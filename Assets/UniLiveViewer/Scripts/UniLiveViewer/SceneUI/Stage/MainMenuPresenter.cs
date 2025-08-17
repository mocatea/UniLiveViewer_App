using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.Player;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// 全体とactor以外用（未整理）
    /// </summary>
    public class MainMenuPresenter : IAsyncStartable, IDisposable
    {
        bool _isRootActive = true;

        readonly PlayerInputService _playerInputService;
        readonly ItemPage _itemPage;
        readonly RootAudioSourceService _audioSourceService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public MainMenuPresenter(
            PlayerInputService playerInputService,
            ItemPage itemPage,
            RootAudioSourceService audioSourceService)
        {
            _playerInputService = playerInputService;
            _itemPage = itemPage;
            _audioSourceService = audioSourceService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _playerInputService.ClickMenuAsObservable()
                .Where(x => x == PlayerHandType.RHand)
                .Subscribe(_ => SwitchEnable()).AddTo(_disposables);

            _itemPage.OnStart();

            await UniTask.CompletedTask;
        }

        void SwitchEnable()
        {
            _isRootActive = !_isRootActive;

            if (_isRootActive) _audioSourceService.PlayOneShot(AudioSE.MenuOpen);
            else _audioSourceService.PlayOneShot(AudioSE.MenuClose);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
