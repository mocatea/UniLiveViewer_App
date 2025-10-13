using Cysharp.Threading.Tasks;
using MessagePipe;
using NanaCiel;
using System;
using System.Threading;
using UniGLTF;
using UniLiveViewer.Actor.AttachPoint;
using UniLiveViewer.Actor.Expression;
using UniLiveViewer.Actor.LookAt;
using UniLiveViewer.External;
using UniLiveViewer.Menu;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using UniVRM10;
using VContainer;
using VContainer.Unity;
using VRM;

namespace UniLiveViewer.Actor
{
    /// <summary>
    /// 識別子がまだないので整理して作る
    /// </summary>
    public class VRMActorService : IActorEntity
    {
        IReactiveProperty<ActorEntity> IActorEntity.ActorEntity() => _actorEntity;
        readonly ReactiveProperty<ActorEntity> _actorEntity = new();

        IReactiveProperty<bool> IActorEntity.Active() => _active;
        readonly ReactiveProperty<bool> _active = new(false);

        /// <summary>
        /// TODO: この通知リレーは止めたい
        /// </summary>
        IReactiveProperty<float> IActorEntity.RootScalar() => _rootScalar;
        readonly ReactiveProperty<float> _rootScalar = new(FileReadAndWriteUtility.UserProfile.InitCharaSize);

        IReactiveProperty<float> IActorEntity.RawRootScalar() => _rawRootScalar;
        readonly ReactiveProperty<float> _rawRootScalar = new();

        IReactiveProperty<ActorState> IActorEntity.ActorState() => _actorState;
        readonly ReactiveProperty<ActorState> _actorState = new(ActorState.NULL);

        /// <summary>
        /// 主に親指定時のスケール問題を解決する目的で
        /// ミニチュア時とLineSelector（こっちは座標上書き目的もある）
        /// </summary>
        Transform _overrideAnchor;

        readonly LifetimeScope _lifetimeScope;
        readonly VRMService _vrmService;
        readonly CharaInfoData _charaInfoData;
        readonly AttachPointService _attachPointService;
        readonly RegisterData _data;
        readonly ILipSync _lipSync;
        readonly IFacialSync _faceSync;
        readonly NormalizedBoneGenerator _normalizedBoneGenerator;
        readonly LookAtService _lookAtService;

        //VRMUI非表示専用
        readonly IPublisher<VRMLoadResultData> _publisher;

        [Inject]
        public VRMActorService(
            LifetimeScope lifetimeScope,
            VRMService vrmService,
            CharaInfoData charaInfoData,
            RegisterData data,
            ILipSync lipSync,
            IFacialSync facialSync,
            NormalizedBoneGenerator normalizedBoneGenerator,
            AttachPointService attachPointService,
            IPublisher<VRMLoadResultData> publisher,
            LookAtService lookAtService)
        {
            _lifetimeScope = lifetimeScope;
            _vrmService = vrmService;
            _charaInfoData = charaInfoData;
            _attachPointService = attachPointService;
            _publisher = publisher;

            _data = data;
            _lipSync = lipSync;
            _faceSync = facialSync;
            _normalizedBoneGenerator = normalizedBoneGenerator;
            _lookAtService = lookAtService;
        }

        public async UniTask SetupAsync(Transform firstParent, CancellationToken cancellation)
        {
            try
            {
                if (_data.LoadVrmAsMode10)
                {
                    var instance = await _vrmService.Load10Async(_data.FullPath, cancellation);//1.0
                    await SetupInternalAsync(instance, cancellation);
                }
                else
                {
                    var instance = await _vrmService.LoadAsync(_data.FullPath, cancellation);
                    await SetupInternalAsync(instance, cancellation);
                }
                SetState(ActorState.MINIATURE, firstParent);
                _lifetimeScope.transform.localPosition = Vector3.zero;
                _lifetimeScope.transform.localRotation = Quaternion.identity;

                _publisher.Publish(new VRMLoadResultData(this));
            }
            catch
            {
                _publisher.Publish(new VRMLoadResultData(null));
            }
        }

        async UniTask SetupInternalAsync(Vrm10Instance instance, CancellationToken cancellation)
        {
            var go = instance.gameObject;
            go.transform.SetParent(_lifetimeScope.transform, false);
            go.name = instance.Vrm.Meta.Name;
            go.layer = Constants.LayerNoGrabObject;//オートカメラ識別にも利用

            // 0.XはmatConverter内で行っているが1.0はないのでここで
            var skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var mesh in skinnedMeshRenderers)
            {
                if (mesh.transform.name.Contains("eye", StringComparison.OrdinalIgnoreCase)
                    || mesh.transform.name.Contains("face", StringComparison.OrdinalIgnoreCase))
                {
                    //目や顔にアウトラインは残念な感じになりやすいので
                    mesh.gameObject.layer = Constants.LayerActorFace;
                }
                else mesh.gameObject.layer = go.layer;
            }

            // 表情系
            var runtimeExpression = instance.Runtime.Expression;
            _lipSync.Setup(instance.transform, expression: runtimeExpression);
            _faceSync.Setup(instance.transform, expression: runtimeExpression);

            await UniTask.Delay(100);// TODO: 最後に調整

            _charaInfoData.viewName = go.name;
            var vmdPlayer = go.AddComponent<VMDPlayer_Custom>();
            var charaInfoData = GameObject.Instantiate(_charaInfoData);
            vmdPlayer.Initialize(charaInfoData, _faceSync, _lipSync);

            _actorEntity.Value = new ActorEntity(instance.GetComponent<Animator>(),
                _charaInfoData, vmdPlayer, _lookAtService, _normalizedBoneGenerator);

            await _attachPointService.SetupAsync(_actorEntity.Value.BoneMap, cancellation);

            var runtimeGltfInstance = instance.GetComponent<RuntimeGltfInstance>();
            runtimeGltfInstance.EnableUpdateWhenOffscreen(); // Mesh消え対策
            runtimeGltfInstance.ShowMeshes();

            // 各serviceのUpdate系が始動
            _active.Value = true;
        }

        async UniTask SetupInternalAsync(RuntimeGltfInstance instance, CancellationToken cancellation)
        {
            var go = instance.gameObject;
            go.transform.SetParent(_lifetimeScope.transform, false);
            go.name = go.GetComponent<VRMMeta>().Meta.Title;
            go.layer = Constants.LayerNoGrabObject;//オートカメラ識別にも利用

            // 表情系
            var vrmBlendShape = go.GetComponent<VRMBlendShapeProxy>();
            _lipSync.Setup(instance.transform, vrmBlendShape);
            _faceSync.Setup(instance.transform, vrmBlendShape);

            await UniTask.Delay(100);// TODO: 最後に調整

            // マテリアルURP化
            var materialConverter = (IMaterialConverter)new MaterialConverter(go.layer);
            await materialConverter.Convert(instance.SkinnedMeshRenderers, cancellation);

            //使わないようにする
            go.AddComponent<MaterialManager>();

            //AttachPointとか追加される前にmeshrenderのみマテリアル調整
            var meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers?.Length > 0)
            {
                await materialConverter.Conversion_Item(meshRenderers, cancellation).OnError();
            }

            _charaInfoData.viewName = go.name;

            var vmdPlayer = go.AddComponent<VMDPlayer_Custom>();
            var charaInfoData = GameObject.Instantiate(_charaInfoData);
            vmdPlayer.Initialize(charaInfoData, _faceSync, _lipSync);

            _actorEntity.Value = new ActorEntity(instance.GetComponent<Animator>(),
                _charaInfoData, vmdPlayer, _lookAtService, _normalizedBoneGenerator);

            await _attachPointService.SetupAsync(_actorEntity.Value.BoneMap, cancellation);

            instance.EnableUpdateWhenOffscreen(); // Mesh消え対策
            instance.ShowMeshes();

            // 各serviceのUpdate系が始動
            _active.Value = true;
        }

        void IActorEntity.Activate(bool isActive)
        {
            _active.Value = isActive;

            if (_lifetimeScope.gameObject.activeSelf == isActive) return;
            _lifetimeScope.gameObject.SetActive(isActive);
        }

        void IActorEntity.SetRootTransform(Vector3 pos, Quaternion quaternion)
        {
            _lifetimeScope.gameObject.transform.SetPositionAndRotation(pos, quaternion);
        }

        void IActorEntity.AddRootScalar(float add)
        {
            _rootScalar.Value = Mathf.Clamp(_rootScalar.Value + add, ActorConstants.ActorMinSize, ActorConstants.ActorMaxSize);
            _lifetimeScope.transform.localScale = Vector3.one * _rootScalar.Value;
            //_actorEntity.Value.GetAnimator.transform.localScale = Vector3.one * _customScalar;
        }

        /// <summary>
        /// 状態設定
        /// </summary>
        /// <param name="setState"></param>
        /// <param name="overrideTarget">対象位置に座標を合わせる</param>
        public void SetState(ActorState setState, Transform overrideTarget)
        {
            if (_actorEntity.Value == null) return;
            if (_actorState.Value == setState) return;

            var rootGameObject = _lifetimeScope.gameObject;
            var globalScale = Vector3.zero;

            _overrideAnchor = overrideTarget;

            switch (setState)
            {
                case ActorState.NULL:
                    //VRMとPrefab用
                    globalScale = Vector3.one;
                    rootGameObject.layer = Constants.LayerNoDefault;

                    // TODO: 主にVRMPrefab化の時用なので後でどうにかする
                    //リセットして無効化しておく
                    //LookAtVRM.EyeReset();
                    //LookAtVRM.SetEnable(false);
                    //SetLookAt(false);

                    break;
                case ActorState.MINIATURE:
                    globalScale = Vector3.one * 0.18f;
                    rootGameObject.layer = Constants.LayerNoGrabObject;
                    break;
                case ActorState.HOLD:
                    globalScale = Vector3.one * 0.18f;
                    break;
                case ActorState.ON_CIRCLE:
                    globalScale = Vector3.one;
                    break;
                case ActorState.FIELD:
                    globalScale = Vector3.one;
                    rootGameObject.layer = Constants.LayerNoFieldObject;
                    break;
            }

            //座標の上書き設定
            var rootTransform = _lifetimeScope.transform;
            if (_overrideAnchor)
            {
                rootTransform.parent = _overrideAnchor;

                //親Scaleの影響を無視する為に算出
                globalScale.x *= 1 / _overrideAnchor.lossyScale.x;
                globalScale.y *= 1 / _overrideAnchor.lossyScale.y;
                globalScale.z *= 1 / _overrideAnchor.lossyScale.z;

                rootTransform.position = _overrideAnchor.position;
                rootTransform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else rootTransform.parent = null;

            if (setState == ActorState.MINIATURE || setState == ActorState.HOLD)
            {
                rootTransform.localScale = globalScale;
            }
            else
            {
                rootTransform.localScale = globalScale * _rootScalar.Value;
            }

            _actorState.Value = setState;
            _rawRootScalar.Value = rootTransform.localScale.x;
        }

        void IActorEntity.OnTick()
        {
            //OVRgrab側で掴まれている為、常時上書き
            if (_actorState.Value == ActorState.ON_CIRCLE)
            {
                _lifetimeScope.transform.position = _overrideAnchor.position;
                _lifetimeScope.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }

        void IActorEntity.Delete()
        {
            GameObject.Destroy(_lifetimeScope.gameObject);
        }
    }
}
