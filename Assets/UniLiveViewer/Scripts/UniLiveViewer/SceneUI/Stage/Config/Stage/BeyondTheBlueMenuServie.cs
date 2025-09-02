using System;
using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class BeyondTheBlueMenuServie : IStageMenuService
    {
        const int PropSetCount = 4;

        public IReactiveProperty<int> PropSet => _propSet;
        readonly ReactiveProperty<int> _propSet = new(1);

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
            _settings.PropSetButtons[0].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickPropSetButton(-1)).AddTo(_disposables);
            _settings.PropSetButtons[1].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickPropSetButton(1)).AddTo(_disposables);

            _settings.GodRayButton.OnTriggerAsObservable()
                .Select(x => x.isEnable)
                .Subscribe(OnClickGodRay).AddTo(_disposables);

            _settings.WaterColorSlider.ValueAsObservable
                .Subscribe(OnChangeWaterColor).AddTo(_disposables);

            _settings.GodRayButton.isEnable = true;
            _settings.WaterColorSlider.Value = _waterColor.Value;

            _settings.PropSetText.text = _propSet.Value == 0 ? "None" : _propSet.Value.ToString();
        }

        void IStageMenuService.OnEnable()
        {

        }

        void OnClickPropSetButton(int moveIndex)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _propSet.Value += moveIndex;
            if (_propSet.Value < 0) _propSet.Value = PropSetCount;
            else if (_propSet.Value > PropSetCount) _propSet.Value = 0;

            var text = _propSet.Value == 0 ? "None" : _propSet.Value.ToString();
            _settings.PropSetText.text = text;
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