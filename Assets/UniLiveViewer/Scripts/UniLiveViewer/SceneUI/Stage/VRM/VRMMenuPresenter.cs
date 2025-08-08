using Cysharp.Threading.Tasks;
using MessagePipe;
using NanaCiel;
using System;
using System.Threading;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class VRMMenuPresenter : IAsyncStartable, IDisposable
    {
        /// <summary>
        /// サムネページ処理停止用
        /// </summary>
        CancellationTokenSource _cts;

        readonly IPublisher<VRMMenuShowMessage> _publisher;
        readonly ISubscriber<VRMMenuShowMessage> _menuShowSubscriber;
        readonly VRMSwitchController _vrmSwitchController;
        readonly VRMMenuRootService _vrmMenuRootService;
        readonly ThumbnailService _thumbnailService;
        readonly CharacterPage _characterPage;
        readonly FileAccessManager _fileAccessManager;
        readonly RootAudioSourceService _rootAudioSourceService;
        readonly TextureAssetManager _textureAssetManager;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public VRMMenuPresenter(
            IPublisher<VRMMenuShowMessage> publisher,
            ISubscriber<VRMMenuShowMessage> menuShowSubscriber,
            VRMSwitchController vrmSwitchController,
            VRMMenuRootService vrmMenuRootService,
            ThumbnailService thumbnailService,
            FileAccessManager fileAccessManager,
            RootAudioSourceService rootAudioSourceService,
            TextureAssetManager textureAssetManager,
            CharacterPage characterPage)
        {
            _publisher = publisher;
            _menuShowSubscriber = menuShowSubscriber;
            _vrmSwitchController = vrmSwitchController;
            _vrmMenuRootService = vrmMenuRootService;
            _thumbnailService = thumbnailService;
            _fileAccessManager = fileAccessManager;
            _rootAudioSourceService = rootAudioSourceService;
            _textureAssetManager = textureAssetManager;
            _characterPage = characterPage;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _menuShowSubscriber
                .Subscribe(async x =>
                {
                    _cts?.Cancel();//ページ状態更新と見なす
                    var isClose = x.PageIndex == -1;

                    _vrmMenuRootService.SetEnableRoot(!isClose);
                    if (!isClose) _vrmSwitchController.InitPage(0);
                    if (isClose) return;
                    if (x.PageIndex == 0)
                    {
                        _cts = new CancellationTokenSource();
                        await _thumbnailService.BeginAsync(_cts.Token).IgnoreCancellationException();
                    }
                }).AddTo(_disposables);

            _thumbnailService.OnClickAsObservable
                .Subscribe(x =>
                {
                    _characterPage.OnClickThumbnail();
                }).AddTo(_disposables);
            await _vrmSwitchController.InitializeAsync(_fileAccessManager, _rootAudioSourceService, _textureAssetManager, _publisher, cancellation);
            await _thumbnailService.InitializeAsync(cancellation);
            _vrmMenuRootService.SetEnableRoot(false);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
