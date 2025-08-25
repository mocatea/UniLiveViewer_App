using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.SceneSelect
{
    public class SceneSelectMenuPresenter : IStartable, IDisposable
    {
        readonly IPublisher<PlayerInputOperationMessage> _playerInputOperationPublisher;
        readonly SceneSelectMenuService _sceneSelectMenuService;
        readonly SceneSelectMenuSettings _settings;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SceneSelectMenuPresenter(
            IPublisher<PlayerInputOperationMessage> playerInputOperationPublisher,
            SceneSelectMenuService sceneSelectMenuService,
            SceneSelectMenuSettings settings)
        {
            _playerInputOperationPublisher = playerInputOperationPublisher;
            _sceneSelectMenuService = sceneSelectMenuService;
            _settings = settings;
        }

        void IStartable.Start()
        {
            _settings.ChangeSceneAsObservable
                .Subscribe(x =>
                {
                    _sceneSelectMenuService.OnChangeSceneAsync(x).Forget();
                    _playerInputOperationPublisher.Publish(new PlayerInputOperationMessage(false));
                }).AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }

}