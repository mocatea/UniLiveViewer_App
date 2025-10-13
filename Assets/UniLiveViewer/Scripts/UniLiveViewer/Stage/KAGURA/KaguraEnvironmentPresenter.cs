using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Menu.Config.Stage;
using UniLiveViewer.MessagePipe;
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
        readonly ISubscriber<PassthroughMessage> _passthroughSubscriber;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public KaguraEnvironmentPresenter(
            IStageMenuService stageMenuServie,
            StageLightingService stageLightingService,
            KaguraEnvironmentService environmentService,
            ISubscriber<PassthroughMessage> passthroughSubscriber)
        {
            _stageMenuServie = stageMenuServie;
            _stageLightingService = stageLightingService;
            _environmentService = environmentService;
            _passthroughSubscriber = passthroughSubscriber;
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

                // TODO: 親LSが非アクティブで初回反応できない問題
                _passthroughSubscriber
                    .Subscribe(x => _environmentService.OnChangePassthrough(x.IsEnable))
                    .AddTo(_disposable);
            }
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}