using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Menu.Config.Stage;
using UniLiveViewer.Timeline;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class GymnasiumEnvironmentPresenter : IStartable, ITickable, IDisposable
    {
        readonly IStageMenuService _stageMenuServie;
        readonly GymnasiumEnvironmentService _environmentService;
        readonly PlayableBinderService _playableBinderService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public GymnasiumEnvironmentPresenter(
            IStageMenuService stageMenuServie,
            GymnasiumEnvironmentService environmentService,
            PlayableBinderService playableBinderService)
        {
            _stageMenuServie = stageMenuServie;
            _environmentService = environmentService;
            _playableBinderService = playableBinderService;
        }

        void IStartable.Start()
        {
            _playableBinderService.StageActorCount
                .Subscribe(_environmentService.OnChangeSummonedCount)
                .AddTo(_disposable);

            // 一旦Downcast、乱用しすぎたらイベント集約パターンにする
            if (_stageMenuServie is GymnasiumMenuServie menuServie)
            {
                menuServie.StageLightIndexAsObservable
                    .Subscribe(_environmentService.OnChangeStageLight)
                    .AddTo(_disposable);
                menuServie.StageLightIsWhiteAsObservable
                    .Subscribe(_environmentService.OnClickWhiteLightColor)
                    .AddTo(_disposable);
            }

            _environmentService.Begin();
        }

        void ITickable.Tick()
        {
            _environmentService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}