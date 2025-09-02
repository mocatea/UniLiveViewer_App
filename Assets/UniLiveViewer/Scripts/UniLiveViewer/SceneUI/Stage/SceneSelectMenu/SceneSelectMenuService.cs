using Cysharp.Threading.Tasks;
using System.Threading;
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

        [Inject]
        public SceneSelectMenuService(
            MenuRootService menuRootService,
            SceneChangeService sceneChangeService,
            TimelineService timelineService,
            RootAudioSourceService rootAudioSourceService,
            BlackoutCurtain blackoutCurtain)
        {
            _menuRootService = menuRootService;
            _sceneChangeService = sceneChangeService;
            _timelineService = timelineService;
            _rootAudioSourceService = rootAudioSourceService;
            _blackoutCurtain = blackoutCurtain;
        }

        public async UniTask OnChangeSceneAsync(SceneType sceneType)
        {
            _rootAudioSourceService.PlayOneShot(AudioSE.ButtonClick);

            if (sceneType == SceneType.FANTASY_VILLAGE) return;//一旦無効化

            var dummy = new CancellationToken();
            await _timelineService.PauseAsync(dummy);// 音が割れるので止める

            await UniTask.Delay(100, cancellationToken: dummy);
            _menuRootService.OnMenuSwitching();//開いてる想定なので閉じる

            _rootAudioSourceService.PlayOneShot(AudioSE.SceneTransition);
            _blackoutCurtain.Closing();
            await _sceneChangeService.ChangeAsync(sceneType, dummy);
        }
    }
}