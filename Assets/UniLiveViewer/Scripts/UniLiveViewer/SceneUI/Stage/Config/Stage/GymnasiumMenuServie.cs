using System;
using UniLiveViewer.Stage;
using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class GymnasiumMenuServie : IStageMenuService
    {
        public IObservable<int> StageLightIndexAsObservable => _stageLightIndex;
        readonly Subject<int> _stageLightIndex = new();
        public IObservable<bool> StageLightIsWhiteAsObservable => _stageLightIsWhite;
        readonly Subject<bool> _stageLightIsWhite = new();

        readonly GymnasiumMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;

        [Inject]
        public GymnasiumMenuServie(GymnasiumMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.LightColorButton.onTrigger += (btn) => OnClickLightColor(btn.isEnable);

            for (int i = 0; i < _settings.SpotLightButtons.Length; i++)
            {
                _settings.SpotLightButtons[i].onTrigger += (btn) =>
                {
                    var index = btn == _settings.SpotLightButtons[0] ? 0 : 1;// やらんと発火時はLength値になる
                    OnClickSpotLight(index);
                };
            }
        }

        void IStageMenuService.OnEnable()
        {
            _settings.LightColorButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;
            // 明示的通知で初期化
            _stageLightIndex.OnNext(_lightIndex);
            _stageLightIsWhite.OnNext(_settings.LightColorButton.isEnable);
        }

        void OnClickLightColor(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            _stageLightIsWhite.OnNext(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_gym_whitelight = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        // 雑
        int _lightIndex = StageEnums.StageLightDefaultIndex;
        void OnClickSpotLight(int index)
        {
            _audioSourceService.PlayOneShot(AudioSE.SpotlightSwitch);

            var moveIndex = index == 0 ? -1 : 1;
            _lightIndex += moveIndex;
            var max = Enum.GetValues(typeof(StageEnums.StageLight)).Length;
            if (max <= _lightIndex) _lightIndex = 0;
            else if (_lightIndex < 0) _lightIndex = max - 1;

            _stageLightIndex.OnNext(_lightIndex);
            _settings.Texts[0].text = $"SpotLight_{Enum.GetName(typeof(StageEnums.StageLight), _lightIndex)}";
        }

        void IStageMenuService.Dispose()
        {
            _settings.LightColorButton.onTrigger -= (btn) => OnClickLightColor(btn.isEnable);
            for (int i = 0; i < _settings.SpotLightButtons.Length; i++)
            {
                _settings.SpotLightButtons[i].onTrigger -= (btn) => OnClickSpotLight(i);
            }
        }
    }
}