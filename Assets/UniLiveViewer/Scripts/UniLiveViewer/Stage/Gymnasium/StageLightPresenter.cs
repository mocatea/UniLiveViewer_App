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
    public class StageLightPresenter : IStartable, ITickable, IDisposable
    {
        readonly IStageMenuService _stageMenuServie;
        readonly StageLightChangeService _changeService;
        readonly PlayableBinderService _playableBinderService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public StageLightPresenter(
            IStageMenuService stageMenuServie,
            StageLightChangeService changeService,
            PlayableBinderService playableBinderService)
        {
            _stageMenuServie = stageMenuServie;
            _playableBinderService = playableBinderService;
            _changeService = changeService;
        }

        void IStartable.Start()
        {
            _playableBinderService.StageActorCount
                .Subscribe(_changeService.OnChangeSummonedCount)
                .AddTo(_disposable);

            // 一旦Downcast、乱用しすぎたらイベント集約パターンにする
            if (_stageMenuServie is GymnasiumMenuServie gymnasiumMenu)
            {
                gymnasiumMenu.StageLightIsWhiteAsObservable
                .Subscribe(_changeService.OnChangeLightColor)
                .AddTo(_disposable);
                gymnasiumMenu.StageLightIndexAsObservable
                    .Subscribe(_changeService.OnChangeStageLight)
                    .AddTo(_disposable);
            }
        }

        void ITickable.Tick()
        {
            _changeService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}