using System;
using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class BeyondTheBlueMenuServie : IStageMenuService
    {
        public IObservable<bool> IsGodRayAsObservable => _isGodRay;
        readonly Subject<bool> _isGodRay = new();

        public IReactiveProperty<float> WaterLevel => _waterLevel;
        readonly ReactiveProperty<float> _waterLevel = new();


        readonly BeyondTheBlueMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public BeyondTheBlueMenuServie(BeyondTheBlueMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.GodRayButton.OnTriggerAsObservable()
                .Select(x => x.isEnable)
                .Subscribe(OnClickGodRay).AddTo(_disposables);

            _settings.WaterLevelSlider.ValueAsObservable
                .Subscribe(OnChangeWaterLevel).AddTo(_disposables);


            _settings.GodRayButton.isEnable = true;
            _waterLevel.Value = 0.3f;
            _settings.Texts[0].text = $"{_waterLevel.Value.ToString("0.00")} m";
        }

        void IStageMenuService.OnEnable()
        {
        }

        void OnClickGodRay(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isGodRay.OnNext(isEnable);
        }

        public void OnChangeWaterLevel(float level)
        {
            _settings.Texts[0].text = $"{level.ToString("0.00")} m";
            _waterLevel.Value = level;
        }

        void IStageMenuService.Dispose()
        {
            _disposables.Dispose();
        }
    }
}