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
        readonly AudioPlaybackPage _audioPlaybackPage;
        readonly ItemPage _itemPage;
        readonly JumpList _jumpList;
        readonly RootAudioSourceService _audioSourceService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public MainMenuPresenter(
            PlayerInputService playerInputService,
            AudioPlaybackPage audioPlaybackPage,
            ItemPage itemPage,
            JumpList jumpList,
            RootAudioSourceService audioSourceService)
        {
            _playerInputService = playerInputService;
            _audioPlaybackPage = audioPlaybackPage;
            _itemPage = itemPage;
            _jumpList = jumpList;
            _audioSourceService = audioSourceService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _playerInputService.ClickMenuAsObservable()
                .Where(x => x == PlayerHandType.RHand)
                .Subscribe(_ => SwitchEnable()).AddTo(_disposables);

            _jumpList.OnSelectAsObservable
                .Subscribe(_audioPlaybackPage.OnJumpSelect).AddTo(_disposables);

            _audioPlaybackPage.StartAsync(cancellation).Forget();
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
