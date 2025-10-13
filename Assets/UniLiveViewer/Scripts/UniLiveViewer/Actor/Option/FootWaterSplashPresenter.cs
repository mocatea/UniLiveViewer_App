using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class FootWaterSplashPresenter : IAsyncStartable, IFixedTickable, IDisposable
    {
        CancellationTokenSource _lifetime = new();

        readonly IActorEntity _actorEntity;
        readonly RootAudioSourceService _rootAudioSourceService;
        readonly FootWaterSplashService _footWaterSplashService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FootWaterSplashPresenter(
            IActorEntity actorEntity,
            RootAudioSourceService rootAudioSourceService,
            FootWaterSplashService footWaterSplashService)
        {
            _actorEntity = actorEntity;
            _rootAudioSourceService = rootAudioSourceService;
            _footWaterSplashService = footWaterSplashService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _actorEntity.ActorEntity()
                .Subscribe(_footWaterSplashService.OnChangeActorEntity)
                .AddTo(_disposables);
            _rootAudioSourceService.FootStepsVolumeRate
                .Subscribe(_footWaterSplashService.SetVolume)
                .AddTo(_disposables);
            _actorEntity.RootScalar()
                .Subscribe(_footWaterSplashService.OnChangeRootScalar)
                .AddTo(_disposables);

            await UniTask.CompletedTask;
        }

        void IFixedTickable.FixedTick()
        {
            if (!_actorEntity.Active().Value) return;
            _footWaterSplashService.OnFixedTickAsync(_lifetime.Token).Forget();
        }

        void IDisposable.Dispose()
        {
            _lifetime.Cancel();
            _disposables.Dispose();
        }
    }
}


