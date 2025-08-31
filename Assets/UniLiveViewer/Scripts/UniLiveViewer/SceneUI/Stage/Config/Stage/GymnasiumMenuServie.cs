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

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GymnasiumMenuServie(GymnasiumMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.LightColorButton.OnTriggerAsObservable()
                .Subscribe(x => OnClickLightColor(x.isEnable))
                .AddTo(_disposables);

            _settings.SpotLightButtons[0].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickSpotLight(-1)).AddTo(_disposables);
            _settings.SpotLightButtons[1].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickSpotLight(1)).AddTo(_disposables);
        }

        void IStageMenuService.OnEnable()
        {
            _settings.Texts[0].text = $"{Enum.GetName(typeof(StageEnums.StageLight), _lightIndex)}";
            _settings.LightColorButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;
            // 明示的通知で初期化
            _stageLightIndex.OnNext(_lightIndex);
            _stageLightIsWhite.OnNext(_settings.LightColorButton.isEnable);
        }

        void OnClickLightColor(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _stageLightIsWhite.OnNext(isEnable);
        }

        // 雑
        int _lightIndex = StageEnums.StageLightDefaultIndex;
        void OnClickSpotLight(int moveIndex)
        {
            _audioSourceService.PlayOneShot(AudioSE.SpotlightSwitch);
            _lightIndex += moveIndex;
            var max = Enum.GetValues(typeof(StageEnums.StageLight)).Length;
            if (max <= _lightIndex) _lightIndex = 0;
            else if (_lightIndex < 0) _lightIndex = max - 1;

            _settings.Texts[0].text = $"{Enum.GetName(typeof(StageEnums.StageLight), _lightIndex)}";
            _stageLightIndex.OnNext(_lightIndex);
        }

        void IStageMenuService.Dispose()
        {
            _disposables.Dispose();
        }
    }
}