using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// TODO: LS化する
    /// </summary>
    public class AutoCamera : MonoBehaviour
    {
        enum SWITCHTYPE
        {
            ALL,
            RANDOM_ONE,
        }

        public bool _isUpdate = true;
        [Header("＜共通＞")]
        /// <summary> 
        /// 撮影禁止角度
        /// 下向きすぎると地面やスカートにめり込むので更新なし
        /// </summary>
        [SerializeField] float _prohibitedAngle = 135f;
        [SerializeField] int _interval = 5000;
        [SerializeField] SWITCHTYPE _switchType = SWITCHTYPE.ALL;//カメラ候補を切り替えるモード
        [SerializeField] Camera[] _cameras;

        void Start()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            foreach (var camera in _cameras)
            {
                camera.enabled = false;
            }

            AutoUpdate(cancellationToken).Forget();
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
                switch (_switchType)
                {
                    case SWITCHTYPE.ALL:
                        foreach (var camera in _cameras) camera.Render();
                        break;
                    case SWITCHTYPE.RANDOM_ONE:
                        var pickedCameraIndex = Random.Range(0, _cameras.Length);
                        _cameras[pickedCameraIndex].Render();
                        break;
                }
            }
        }
    }
}