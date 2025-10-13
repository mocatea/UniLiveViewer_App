using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// TODO: LS化する
    /// </summary>
    public class RandomAngleAutoCamera : MonoBehaviour
    {
        public bool _isUpdate = true;
        /// <summary> VirtualHead </summary>
        [SerializeField] Transform _target;
        /// <summary> 
        /// 撮影禁止角度
        /// 下向きすぎると地面やスカートにめり込むので更新なし
        /// </summary>
        [SerializeField] float _prohibitedAngle = 135f;
        [SerializeField] int _interval = 5000;
        [SerializeField] float _headForwardOffset = 1.5f;
        [SerializeField] float _offsetUp, _offsetDown, _offsetRight, _offsetLeft;
        [SerializeField] Camera _camera;
        [SerializeField] SpriteRenderer[] _spr;
        PlayableBinderService _playableBinderService;

        void Start()
        {
            // NOTE: globalにchara[]取れないと厳しいので配下にするか、ユニーク設定はinterface
            var container = LifetimeScope.Find<TimelineLifetimeScope>().Container;
            _playableBinderService = container.Resolve<PlayableBinderService>();
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            _playableBinderService.BindingToAsObservable
                    .Subscribe(_ => Setup())
                    .AddTo(this);

            _camera.enabled = false;

            AutoUpdate(cancellationToken).Forget();
        }

        /// <summary>
        /// ポータル以外の一番若いindexアクターを被写体に設定
        /// </summary>
        void Setup()
        {
            if (_target) return;
            for (int i = 0; i < _playableBinderService.BindingData.Count; i++)
            {
                if (i == TimelineConstants.PortalIndex) continue;
                var data = _playableBinderService.BindingData[i];
                if (data == null) continue;
                var boneGenerator = data.ActorEntity.ActorEntity().Value.NormalizedBoneGenerator;
                _target = boneGenerator.VirtualHead;
            }
        }

        void OnDrawGizmos()
        {
            if (_target == null) return;
            var headForward = _target.position + (_target.forward * _headForwardOffset);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(headForward, 0.1f);
        }

        async UniTask AutoUpdate(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                // MEMO: アクター移動後かつ同フレームの最後に撮影しないとズレるケースがある
                await UniTask.Delay(
                    _interval,
                    DelayType.DeltaTime,
                    PlayerLoopTiming.LastTimeUpdate,
                    cancellation);
                if (!_isUpdate) continue;

                //スクリーンに一瞬反映させる
                ShootRandomAngle();
            }
        }


        void ShootRandomAngle()
        {
            Vector3 pos;

            if (_target)
            {
                var angle = Vector3.Angle(_target.forward, Vector3.up);
                if (angle > _prohibitedAngle) return;

                pos = _target.position + (_target.forward * _headForwardOffset);
                pos += _target.right * Random.Range(-_offsetLeft, _offsetRight);
                pos += _target.up * Random.Range(-_offsetDown, _offsetUp);
                _camera.transform.position = pos;
                _camera.transform.forward = _target.position - _camera.transform.position;
            }

            var pickedSprIndex = Random.Range(0, _spr.Length);
            for (int i = 0; i < _spr.Length; i++) _spr[i].enabled = (i == pickedSprIndex);

            _camera.Render();
        }

    }
}