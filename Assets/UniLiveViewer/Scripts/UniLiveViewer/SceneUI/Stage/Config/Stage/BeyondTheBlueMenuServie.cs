using System;
using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class BeyondTheBlueMenuServie : IStageMenuService
    {
        public IObservable<bool> IsGodRayAsObservable => _isGodRay;
        readonly Subject<bool> _isGodRay = new();

        public IReactiveProperty<float> WaterColor => _waterColor;
        readonly ReactiveProperty<float> _waterColor = new(0.58f);//水色


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

            _settings.WaterColorSlider.ValueAsObservable
                .Subscribe(OnChangeWaterColor).AddTo(_disposables);


            _settings.GodRayButton.isEnable = true;
            _settings.WaterColorSlider.Value = _waterColor.Value;
        }

        void IStageMenuService.OnEnable()
        {
        }

        void OnClickGodRay(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isGodRay.OnNext(isEnable);
        }

        public void OnChangeWaterColor(float level)
        {
            _waterColor.Value = level;
        }

        void IStageMenuService.Dispose()
        {
            _disposables.Dispose();
        }
    }
}