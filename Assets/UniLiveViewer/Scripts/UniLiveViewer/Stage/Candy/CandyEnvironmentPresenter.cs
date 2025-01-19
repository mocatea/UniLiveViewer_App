using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Menu.Config.Stage;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Candy
{
    public class CandyEnvironmentPresenter : IStartable, IDisposable
    {
        readonly IStageMenuService _stageMenuServie;
        readonly CandyEnvironmentService _environmentService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public CandyEnvironmentPresenter(
            IStageMenuService stageMenuServie,
            CandyEnvironmentService environmentService)
        {
            _stageMenuServie = stageMenuServie;
            _environmentService = environmentService;
        }

        void IStartable.Start()
        {
            if (_stageMenuServie is CandyLiveMenuServie candyLiveMenuServie)
            {
                candyLiveMenuServie.IsParticleAsObservable
                    .Subscribe(_environmentService.OnClickParticle)
                    .AddTo(_disposable);
                candyLiveMenuServie.IsLaserGunAsObservable
                    .Subscribe(_environmentService.OnClickLaserGun)
                    .AddTo(_disposable);
                candyLiveMenuServie.IsReflectionAsObservable
                    .Subscribe(_environmentService.OnClickReflection)
                    .AddTo(_disposable);
                candyLiveMenuServie.IsSonicBoomAsObservable
                    .Subscribe(_environmentService.OnClickSonicBoom)
                    .AddTo(_disposable);
                candyLiveMenuServie.IsPlayManualAsObservable
                    .Subscribe(_environmentService.OnClickPlayManual)
                    .AddTo(_disposable);
            }
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}