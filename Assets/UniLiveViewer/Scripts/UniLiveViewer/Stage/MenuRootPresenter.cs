using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniLiveViewer.Player;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuRootPresenter : IAsyncStartable, IDisposable
    {
        readonly FileAccessManager _fileAccessManager;
        readonly MenuRootService _menuRootService;
        readonly PlayerInputService _playerInputService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public MenuRootPresenter(
            FileAccessManager fileAccessManager,
            MenuRootService menuRootService,
            PlayerInputService playerInputService)
        {
            _fileAccessManager = fileAccessManager;
            _menuRootService = menuRootService;
            _playerInputService = playerInputService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _menuRootService.Initialize();

            _fileAccessManager.EndLoadingAsObservable
                .Delay(TimeSpan.FromSeconds(1))
                .Subscribe(_ => _menuRootService.OnLoadEndAsync(cancellation).Forget())
                .AddTo(_disposables);
            _playerInputService.ClickMenuAsObservable()
                .Where(x => x == PlayerHandType.RHand)
                .Subscribe(_ => _menuRootService.OnMenuSwitching())
                .AddTo(_disposables);

            await UniTask.CompletedTask;
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
