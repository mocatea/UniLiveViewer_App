using System;
using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Sound
{
    public class SoundMenuService : IDisposable
    {
        public IReadOnlyReactiveProperty<float> Master => _master;
        readonly ReactiveProperty<float> _master = new();

        public IReadOnlyReactiveProperty<float> BGM => _bgm;
        readonly ReactiveProperty<float> _bgm = new();

        public IReadOnlyReactiveProperty<float> SE => _se;
        readonly ReactiveProperty<float> _se = new();

        public IReadOnlyReactiveProperty<float> Ambient => _ambient;
        readonly ReactiveProperty<float> _ambient = new();

        public IReadOnlyReactiveProperty<float> SpectrumGain => _spectrumGain;
        readonly ReactiveProperty<float> _spectrumGain = new();
        public IReadOnlyReactiveProperty<float> SpectrumSmoothness => _spectrumSmoothness;
        readonly ReactiveProperty<float> _spectrumSmoothness = new();

        public IReadOnlyReactiveProperty<float> FootSteps => _footSteps;
        readonly ReactiveProperty<float> _footSteps = new();

        readonly SoundMenuSettings _settings;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SoundMenuService(SoundMenuSettings settings)
        {
            _settings = settings;
        }

        public void Initialize(
            float masterVolume,
            float bgmVolume,
            float seVolume,
            float ambientVolume,
            float spectrumGain,
            float spectrumSmoothness,
            float footStepsVolume)
        {
            // 購読前に初期化
            {
                _settings.SoundSlider[0].Value = masterVolume;
                _settings.SoundText[0].text = $"{(int)masterVolume}";

                _settings.SoundSlider[1].Value = bgmVolume;
                _settings.SoundText[1].text = $"{(int)bgmVolume}";

                _settings.SoundSlider[2].Value = seVolume;
                _settings.SoundText[2].text = $"{(int)seVolume}";

                _settings.SoundSlider[3].Value = ambientVolume;
                _settings.SoundText[3].text = $"{(int)ambientVolume}";

                _settings.SoundSlider[4].Value = footStepsVolume;
                _settings.SoundText[4].text = $"{(int)footStepsVolume}";

                _settings.SoundSlider[8].Value = spectrumGain;
                _settings.SoundText[8].text = $"{(int)spectrumGain}";

                _settings.SoundSlider[9].Value = spectrumSmoothness;
                _settings.SoundText[9].text = $"{(int)spectrumSmoothness}";
            }

            // スライダー値 0～100(%)
            _settings.SoundSlider[0].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.SoundText[0].text = $"{(int)x}";
                    _master.Value = x;
                }).AddTo(_disposables);

            _settings.SoundSlider[1].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.SoundText[1].text = $"{(int)x}";
                    _bgm.Value = x;
                }).AddTo(_disposables);
            _settings.SoundSlider[2].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.SoundText[2].text = $"{(int)x}";
                    _se.Value = x;
                }).AddTo(_disposables);
            _settings.SoundSlider[3].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.SoundText[3].text = $"{(int)x}";
                    _ambient.Value = x;
                }).AddTo(_disposables);
            _settings.SoundSlider[4].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.SoundText[4].text = $"{(int)x}";
                    _footSteps.Value = x;
                }).AddTo(_disposables);
            //6未実装
            //7未実装
            _settings.SoundSlider[8].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.SoundText[8].text = $"{(int)x}";
                    _spectrumGain.Value = x;
                }).AddTo(_disposables);
            _settings.SoundSlider[9].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.SoundText[9].text = $"{(int)x}";
                    _spectrumSmoothness.Value = x;
                }).AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}