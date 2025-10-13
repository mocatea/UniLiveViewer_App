using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage;
using UniLiveViewer.Timeline;
using VContainer;

namespace UniLiveViewer.Menu.SceneSelect
{
    public class SceneSelectMenuService
    {
        readonly MenuRootService _menuRootService;
        readonly SceneChangeService _sceneChangeService;
        readonly TimelineService _timelineService;
        readonly RootAudioSourceService _rootAudioSourceService;
        readonly BlackoutCurtain _blackoutCurtain;
        readonly IPublisher<PlayerInputOperationMessage> _playerInputOperationPublisher;

        [Inject]
        public SceneSelectMenuService(
            MenuRootService menuRootService,
            SceneChangeService sceneChangeService,
            TimelineService timelineService,
            RootAudioSourceService rootAudioSourceService,
            BlackoutCurtain blackoutCurtain,
            IPublisher<PlayerInputOperationMessage> playerInputOperationPublisher)
        {
            _menuRootService = menuRootService;
            _sceneChangeService = sceneChangeService;
            _timelineService = timelineService;
            _rootAudioSourceService = rootAudioSourceService;
            _blackoutCurtain = blackoutCurtain;
            _playerInputOperationPublisher = playerInputOperationPublisher;
        }

        public async UniTask OnChangeSceneAsync(SceneType sceneType, CancellationToken cancellation)
        {
            _rootAudioSourceService.PlayOneShot(AudioSE.ButtonClick);

            if (sceneType == SceneType.FANTASY_VILLAGE)
            {
                return;
            }
            else if (sceneType == SceneType.BEYOND_THE_BLUE)
            {
                if (!SystemInfo.IsHighSpecDevice)
                {
                    return;
                }
            }

            await ChangeSceneAsync(sceneType, cancellation);
        }

        async UniTask ChangeSceneAsync(SceneType sceneType, CancellationToken cancellation)
        {
            // Player操作停止
            _playerInputOperationPublisher.Publish(new PlayerInputOperationMessage(false));

            await _timelineService.PauseAsync(cancellation);// 音が割れるので止める

            await UniTask.Delay(100, cancellationToken: cancellation);
            _menuRootService.OnMenuSwitching();// 開いてる想定なので閉じる

            _rootAudioSourceService.PlayOneShot(AudioSE.SceneTransition);
            _blackoutCurtain.Closing();
            await _sceneChangeService.ChangeAsync(sceneType, cancellation);
        }
    }
}