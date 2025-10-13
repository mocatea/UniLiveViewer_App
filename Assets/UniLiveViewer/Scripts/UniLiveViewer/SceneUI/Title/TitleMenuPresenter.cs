using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage.Title;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.SceneUI.Title
{
    public class TitleMenuPresenter : IInitializable, IAsyncStartable, IDisposable
    {
        readonly TitleMenuService _titleMenuService;
        readonly SceneChangeService _sceneChangeService;
        readonly TitleMenuSettings _titleMenuSettings;
        readonly IPublisher<SceneTransitionMessage> _sceneTransitionPublisher;
        readonly CompositeDisposable _disposable = new();

        [Inject]
        public TitleMenuPresenter(
            TitleMenuService titleMenuService,
            TitleMenuSettings titleMenuSettings,
            SceneChangeService sceneChangeService,
            IPublisher<SceneTransitionMessage> sceneTransitionPublisher)
        {
            _titleMenuService = titleMenuService;
            _titleMenuSettings = titleMenuSettings;
            _sceneChangeService = sceneChangeService;
            _sceneTransitionPublisher = sceneTransitionPublisher;
        }

        void IInitializable.Initialize()
        {
            _sceneChangeService.Initialize();
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _titleMenuSettings.MainMenuButton[0].onClick.AsObservable()
                .Subscribe(async _ =>
                {
                    _sceneTransitionPublisher.Publish(new SceneTransitionMessage());
                    await _titleMenuService.LoadScenesAutoAsync(cancellation);
                })
                .AddTo(_disposable);
            _titleMenuSettings.MainMenuButton[1].onClick.AsObservable()
                .Subscribe(_ =>
                {
                    _titleMenuService.OpenCustomLive();
                })
                .AddTo(_disposable);
            _titleMenuSettings.MainMenuButton[2].onClick.AsObservable()
                .Subscribe(_ => _titleMenuService.OpenLicense())
                .AddTo(_disposable);
            _titleMenuSettings.MainMenuButton[3].onClick.AsObservable()
                .Subscribe(async _ =>
                {
                    _sceneTransitionPublisher.Publish(new SceneTransitionMessage());
                    await _titleMenuService.QuitAppAsync(cancellation);
                })
                .AddTo(_disposable);


            _titleMenuSettings.CustomLiveButton[0].onClick.AsObservable()
                .Subscribe(_ =>
                {
                    _titleMenuService.OpenMainMenu();
                })
                .AddTo(_disposable);
            _titleMenuSettings.CustomLiveButton[1].onClick.AsObservable()
                .Subscribe(_ =>
                {
                    var url = "https://hallowed-poison-97e.notion.site/Advanced-play-e095c8bf209e4ae2adc9446c556a2213";
                    Application.OpenURL(url);
                })
                .AddTo(_disposable);
            _titleMenuSettings.CustomLiveButton[2].onClick.AsObservable()
                .Subscribe(_ =>
                {
                    var grantStoragePermission = new GrantStoragePermission();
                    grantStoragePermission.TryGranting();
                })
                .AddTo(_disposable);


            _titleMenuSettings.LicenseButton[0].onClick.AsObservable()
                .Subscribe(_ => _titleMenuService.OpenMainMenu())
                .AddTo(_disposable);

            _titleMenuService.StartAsync(cancellation).Forget();
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}