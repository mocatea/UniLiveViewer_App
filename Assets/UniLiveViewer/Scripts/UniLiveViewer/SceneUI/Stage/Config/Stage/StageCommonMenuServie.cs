using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageCommonMenuService
    {
        public IReactiveProperty<float> LightIntensity => _lightIntensity;
        readonly ReactiveProperty<float> _lightIntensity = new();

        public IReactiveProperty<float> LightRotation => _lightRotation;
        readonly ReactiveProperty<float> _lightRotation = new();

        readonly StageCommonMenuSettings _settings;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public StageCommonMenuService(StageCommonMenuSettings settings)
        {
            _settings = settings;
        }

        public void Initialize(float lightIntensity, float lightRotationYow)
        {
            _settings.LightIntensitySlider.Value = lightIntensity;
            _lightIntensity.Value = lightIntensity;
            _settings.LightIntensityText.text = $"{lightIntensity:0.00}";

            _settings.LightRotationSlider.Value = lightRotationYow;
            _lightRotation.Value = lightRotationYow;
            _settings.LightRotationText.text = $"{lightRotationYow:000.#}";

            _settings.LightIntensitySlider.ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.LightIntensityText.text = $"{x:0.00}";
                    _lightIntensity.Value = x;
                }).AddTo(_disposables);

            _settings.LightRotationSlider.ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.LightRotationText.text = $"{x:000.#}";
                    _lightRotation.Value = x;
                }).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}