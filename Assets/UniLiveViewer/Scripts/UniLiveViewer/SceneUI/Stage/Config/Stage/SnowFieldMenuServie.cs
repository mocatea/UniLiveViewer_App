using System;
using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class SnowFieldMenuServie : IStageMenuService
    {
        public IObservable<bool> IsGodRayAsObservable => _isGodRay;
        readonly Subject<bool> _isGodRay = new();

        public IReactiveProperty<float> WaterLevel => _waterLevel;
        readonly ReactiveProperty<float> _waterLevel = new();

        readonly SnowFieldMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SnowFieldMenuServie(SnowFieldMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {

        }

        void IStageMenuService.OnEnable()
        {
        }

        void IStageMenuService.Dispose()
        {
            _disposables.Dispose();
        }
    }
}