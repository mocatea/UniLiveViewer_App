using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Menu.Config.Stage;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Viewer
{
    public class ViewerEnvironmentPresenter : IStartable, IDisposable
    {
        readonly IStageMenuService _stageMenuServie;
        readonly ViewerEnvironmentService _environmentService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public ViewerEnvironmentPresenter(
            IStageMenuService stageMenuServie,
            ViewerEnvironmentService environmentService)
        {
            _stageMenuServie = stageMenuServie;
            _environmentService = environmentService;
        }

        void IStartable.Start()
        {
            if (_stageMenuServie is ViewerMenuServie viewerMenuServie)
            {
                viewerMenuServie.ParticleMoveIndexAsObservable
                    .Subscribe(_environmentService.OnClickParticle)
                    .AddTo(_disposable);
                viewerMenuServie.WormHolleMoveIndexAsObservable
                    .Subscribe(_environmentService.OnClickWormHole)
                    .AddTo(_disposable);
                viewerMenuServie.SkyboxMoveIndexAsObservable
                    .Subscribe(_environmentService.OnClickSkyBox)
                    .AddTo(_disposable);
                viewerMenuServie.IsFloorLEDAsObservable
                    .Subscribe(_environmentService.OnClickFloorLED)
                    .AddTo(_disposable);
            }
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}