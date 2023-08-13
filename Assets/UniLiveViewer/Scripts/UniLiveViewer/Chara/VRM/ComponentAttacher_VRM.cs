﻿using System.Collections.Generic;
using UnityEngine;
using VRM;
using UniHumanoid;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using NanaCiel;

namespace UniLiveViewer 
{
    /// <summary>
    /// TODO: Prefab目的のMonoBehaviour変、分解できるはず
    /// </summary>
    public class ComponentAttacher_VRM : MonoBehaviour
    {
        [SerializeField] RuntimeAnimatorController _aniConPrefab;
        [SerializeField] LipSync_VRM _lipSyncPrefab;
        [SerializeField] FacialSync_VRM _faceSyncPrefab;
        [SerializeField] CharaInfoData _charaInfoDataPrefab;
        [SerializeField] AttachPoint _attachPointPrefab;
        IMaterialConverter _materialConverter;
        MaterialManager _matManager;

        public CharaController CharaCon => _charaController;
        CharaController _charaController;

        VRMMeta _meta;
        Transform _targetVRM;
        Animator _animator;
        List<VRMSpringBone> _vrmSpringBones = new List<VRMSpringBone>();

        public async UniTask Init(Transform targetVRM, IReadOnlyList<SkinnedMeshRenderer> skins, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _targetVRM = targetVRM;
            transform.parent = _targetVRM.transform;
            _meta = _targetVRM.GetComponent<VRMMeta>();
            if (_meta is null) return;

            //乱れが生じるのでinstance化まで動かさない
            _vrmSpringBones.AddRange(_targetVRM.transform.Find("secondary").GetComponents<VRMSpringBone>());
            foreach (var e in _vrmSpringBones)
            {
                e.enabled = false;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();

            _animator = _targetVRM.GetComponent<Animator>();
            _charaController = _targetVRM.gameObject.AddComponent<CharaController>();
            //スキンメッシュレンダーの流用
            _charaController.SetSkinnedMeshRenderers(skins);
            //マテリアル関係
            _materialConverter = new MaterialConverter(_targetVRM.gameObject.layer);
            _matManager = _targetVRM.gameObject.AddComponent<MaterialManager>();
        }

        public async UniTask Attachment(VRMTouchColliders touchCollider, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            //いろいろ追加される前にmeshrenderのみマテリアル調整
            var meshRenderers = _targetVRM.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null && meshRenderers.Length > 0)
            {
                await _materialConverter.Conversion_Item(meshRenderers.ToArray(), token).OnError();
                token.ThrowIfCancellationRequested();
            }

            await UniTask.WhenAll(
                CustomizeComponent_Standard(token)
                    .OnError(_ => new Exception("Standard Attacher"))
                , CustomizeComponent_VRM(touchCollider, token))
                    .OnError(_ => new Exception("CustomizeComponent Attacher"));
            token.ThrowIfCancellationRequested();

            //skinの方はVRMから流用
            await _materialConverter.Conversion(_charaController, token).OnError();
            token.ThrowIfCancellationRequested();

            await _matManager.ExtractMaterials(_charaController, token).OnError();
            token.ThrowIfCancellationRequested();
        }

        async UniTask CustomizeComponent_Standard(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();

            //Animation関連の調整
            _animator.runtimeAnimatorController = _aniConPrefab;
            _animator.updateMode = AnimatorUpdateMode.Normal;
            _animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            _animator.applyRootMotion = true;

            //コライダーの追加
            var capcol = _targetVRM.gameObject.AddComponent<CapsuleCollider>();
            capcol.center = new Vector3(0, 0.8f, 0);
            capcol.radius = 0.25f;
            capcol.height = 1.5f;

            //リジットボディの追加
            var rb = _targetVRM.gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = true;
            rb.useGravity = false;

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();

            //掴み関連の追加
            _targetVRM.gameObject.AddComponent<MeshRenderer>();
            _targetVRM.gameObject.AddComponent<OVRGrabbable_Custom>();

            //アタッチポイントの追加
            _targetVRM.gameObject.AddComponent<AttachPointGenerator>().anchorPointPrefab = _attachPointPrefab;

            //特殊表情の追加
            //var specialFacial = vrmModel.AddComponent<SpecialFacial>();
            //specialFacial.SetAudioClip_VRM(specialFaceAudioClip);
        }

        /// <summary>
        /// VRMのコンポーネントをカスタマイズする
        /// </summary>
        async UniTask CustomizeComponent_VRM(VRMTouchColliders touchCollider, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();

            //不要なスクリプトを停止
            Destroy(_targetVRM.GetComponent<HumanPoseTransfer>());
            //_targetVRM.GetComponent<HumanPoseTransfer>().enabled = false;
            //vrmModel.GetComponent<Blinker>().enabled = false;
            //Destroy(_targetVRM.GetComponent<VRMFirstPerson_Custom>());

            var blendShapeProxy = _targetVRM.GetComponent<VRMBlendShapeProxy>();

            //ScriptableObject追加
            _charaController.charaInfoData = Instantiate(_charaInfoDataPrefab);

            //シンク系の追加(ScriptableObject後)
            var lipSyncVRM = Instantiate(_lipSyncPrefab);
            var facialSyncVRM = Instantiate(_faceSyncPrefab);
            _charaController.InitVRMSync(lipSyncVRM, facialSyncVRM, blendShapeProxy);
            _charaController.InitLookAtController();

            //VMDプレイヤー追加(各Sync系の後)
            _targetVRM.gameObject.AddComponent<VMDPlayer_Custom>();
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();

            var name = _meta.Meta.Title;
            _charaController.charaInfoData.vrmID = VRMSwitchController.loadVRMID;
            VRMSwitchController.loadVRMID++;
            if (name != "") _charaController.charaInfoData.viewName = name;
            else _charaController.charaInfoData.viewName = _targetVRM.name;

            //揺れモノ調整
            var colliderList = new List<VRMSpringBoneColliderGroup>();//統合用
            for (int i = 0; i < _vrmSpringBones.Count; i++)
            {
                //各配列をリストに統合
                if (_vrmSpringBones[i].ColliderGroups is VRMSpringBoneColliderGroup[] && _vrmSpringBones[i].ColliderGroups.Length > 0)
                {
                    colliderList.AddRange(_vrmSpringBones[i].ColliderGroups);//既存コライダー
                    colliderList.AddRange(touchCollider.colliders);//追加コライダー(PlayerHand)                                                                                                                                                                              
                                                                   //リストから配列に戻す
                    _vrmSpringBones[i].ColliderGroups = colliderList.ToArray();
                    colliderList.Clear();
                    //登録
                    _charaController.SpringBoneList.Add(_vrmSpringBones[i]);
                }
                else
                {
                    //そのまま追加
                    _vrmSpringBones[i].ColliderGroups = touchCollider.colliders;
                    //登録
                    _charaController.SpringBoneList.Add(_vrmSpringBones[i]);
                }
            }
        }
    }

}