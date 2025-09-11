using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Menu.Config.Stage;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Kagura
{
    public class KaguraEnvironmentPresenter : IStartable, IDisposable
    {
        readonly IStageMenuService _stageMenuServie;
        readonly StageLightingService _stageLightingService;
        readonly KaguraEnvironmentService _environmentService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public KaguraEnvironmentPresenter(
            IStageMenuService stageMenuServie,
            StageLightingService stageLightingService,
            KaguraEnvironmentService environmentService)
        {
            _stageMenuServie = stageMenuServie;
            _stageLightingService = stageLightingService;
            _environmentService = environmentService;
        }

        void IStartable.Start()
        {
            if (_stageMenuServie is KaguraLiveMenuServie menuServie)
            {
                menuServie.IsParticleAsObservable
                    .Subscribe(_environmentService.OnClickParticle)
                    .AddTo(_disposable);
                menuServie.IsReflectionAsObservable
                    .Subscribe(_environmentService.OnClickReflection)
                    .AddTo(_disposable);
                menuServie.IsSeaWavesAsObservable
                    .Subscribe(_environmentService.OnClickSeaWaves)
                    .AddTo(_disposable);
                menuServie.FogDensity
                    .Subscribe(_stageLightingService.ChangeFogDensity)
                    .AddTo(_disposable);
            }
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}