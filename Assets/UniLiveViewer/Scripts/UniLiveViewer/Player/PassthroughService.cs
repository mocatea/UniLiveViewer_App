using MessagePipe;
using UniLiveViewer.MessagePipe;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// パススルーとポスプロ共存できないので無効化しておく
    /// </summary>
    [RequireComponent(typeof(OVRManager))]
    public class PassthroughService : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<bool> IsEnable => _isEnable;
        ReactiveProperty<bool> _isEnable = new(false);

        OVRManager _ovrManager;
        Camera _camera;
        IPublisher<PassthroughMessage> _passthroughPublisher;

        [Inject]
        public void Construct(OVRManager ovrManager, Camera camera,
            IPublisher<PassthroughMessage> passthroughPublisher)
        {
            _camera = camera;
            _ovrManager = ovrManager;
            _passthroughPublisher = passthroughPublisher;
        }

        public void Initialize()
        {
            Switching(false);
        }

        public void Switching(bool isEnable)
        {
            if (isEnable)
            {
                _camera.clearFlags = CameraClearFlags.Color;
                _isEnable.Value = true;
                _ovrManager.isInsightPassthroughEnabled = true;
            }
            else
            {
                var go = GameObject.FindGameObjectsWithTag("Passthrough");
                int max = go.Length;
                for (int i = 0; i < max; i++)
                {
                    Destroy(go[max - i - 1]);
                }

                _camera.clearFlags = CameraClearFlags.Skybox;
                _isEnable.Value = false;
                _ovrManager.isInsightPassthroughEnabled = false;
            }
            _passthroughPublisher.Publish(new PassthroughMessage(isEnable));
        }

        public bool IsInsightPassthroughEnabled()
        {
            return _ovrManager.isInsightPassthroughEnabled;
        }
    }
}