using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Stage
{
    public class MenuRootService
    {
        bool _isEnable;

        readonly RootMenuAnchor _rootMenuAnchor;
        readonly Camera _camera;

        [Inject]
        public MenuRootService(
            RootMenuAnchor rootMenuAnchor,
            Camera camera)
        {
            _rootMenuAnchor = rootMenuAnchor;
            _camera = camera;
        }

        public void Initialize()
        {
            _rootMenuAnchor.gameObject.SetActive(true);
            _rootMenuAnchor.transform.position = new Vector3(0, 10, 0);
        }

        public async UniTask OnLoadEndAsync(CancellationToken cancellationToken)
        {
            OnMenuSwitching(false);
            await UniTask.Delay(1000, cancellationToken: cancellationToken);
            OnMenuSwitching(true);
        }

        public void OnMenuSwitching()
        {
            OnMenuSwitching(!_isEnable);
        }

        void OnMenuSwitching(bool isEnable)
        {
            _isEnable = isEnable;
            _rootMenuAnchor.gameObject.SetActive(isEnable);

            if (!isEnable) return;
            _rootMenuAnchor.transform.position = _camera.transform.TransformPoint(new Vector3(0, -0.45f, 0.32f));
            _rootMenuAnchor.transform.rotation = _camera.transform.rotation * Quaternion.Euler(new Vector3(20, 0, 0));
        }
    }
}