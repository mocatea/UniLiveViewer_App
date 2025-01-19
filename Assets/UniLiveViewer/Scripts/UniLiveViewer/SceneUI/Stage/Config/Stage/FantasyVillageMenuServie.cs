using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class FantasyVillageMenuServie : IStageMenuService
    {
        Transform[] _actionObj = new Transform[1];

        readonly FantasyVillageMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;

        [Inject]
        public FantasyVillageMenuServie(FantasyVillageMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.DirectionalLightButton.onTrigger += (btn) => OnClick(btn.isEnable);

            _actionObj[0] = GameObject.FindGameObjectWithTag("MainLight").transform;
        }

        void IStageMenuService.OnEnable()
        {
            _settings.DirectionalLightButton.isEnable = _actionObj[0].gameObject.activeSelf;
        }

        void OnClick(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[0])
            {
                _actionObj[0].gameObject.SetActive(isEnable);
                FileReadAndWriteUtility.UserProfile.scene_fv_light = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void IStageMenuService.Dispose()
        {
            _settings.DirectionalLightButton.onTrigger -= (btn) => OnClick(btn.isEnable);
        }
    }
}