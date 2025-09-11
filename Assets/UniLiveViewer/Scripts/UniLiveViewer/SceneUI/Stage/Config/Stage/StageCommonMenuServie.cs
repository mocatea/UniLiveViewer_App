using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageCommonMenuService
    {
        public IReactiveProperty<float> LightIntensity => _lightIntensity;
        readonly ReactiveProperty<float> _lightIntensity = new();

        readonly StageCommonMenuSettings _settings;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public StageCommonMenuService(StageCommonMenuSettings settings)
        {
            _settings = settings;
        }

        public void Initialize(float lightIntensity)
        {
            _settings.LightIntensitySlider.Value = lightIntensity;
            _lightIntensity.Value = lightIntensity;
            _settings.LightIntensityText.text = $"{_lightIntensity.Value:0.00}";

            _settings.LightIntensitySlider.ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.LightIntensityText.text = $"{x:0.00}";
                    _lightIntensity.Value = x;
                }).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}