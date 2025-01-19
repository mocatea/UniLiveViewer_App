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
        readonly KaguraEnvironmentService _environmentService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public KaguraEnvironmentPresenter(
            IStageMenuService stageMenuServie,
            KaguraEnvironmentService environmentService)
        {
            _stageMenuServie = stageMenuServie;
            _environmentService = environmentService;
        }

        void IStartable.Start()
        {
            if (_stageMenuServie is KaguraLiveMenuServie kaguraLiveMenuServie)
            {
                kaguraLiveMenuServie.IsParticleAsObservable
                    .Subscribe(_environmentService.OnClickParticle)
                    .AddTo(_disposable);
                kaguraLiveMenuServie.IsReflectionAsObservable
                    .Subscribe(_environmentService.OnClickReflection)
                    .AddTo(_disposable);
                kaguraLiveMenuServie.IsSeaWavesAsObservable
                    .Subscribe(_environmentService.OnClickSeaWaves)
                    .AddTo(_disposable);
                kaguraLiveMenuServie.FogDensity
                    .Subscribe(_environmentService.OnClickFog)
                    .AddTo(_disposable);
            }
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}