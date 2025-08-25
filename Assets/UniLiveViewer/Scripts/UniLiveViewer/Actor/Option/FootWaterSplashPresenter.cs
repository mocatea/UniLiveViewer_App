using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class FootWaterSplashPresenter : IAsyncStartable, ITickable, IDisposable
    {
        CancellationToken _lifetime;

        readonly IActorEntity _actorEntity;
        readonly FootWaterSplashService _footWaterSplashService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FootWaterSplashPresenter(
            IActorEntity actorEntity,
            FootWaterSplashService footWaterSplashService)
        {
            _actorEntity = actorEntity;
            _footWaterSplashService = footWaterSplashService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _lifetime = cancellation;
 
            _actorEntity.ActorEntity()
                .Subscribe(_footWaterSplashService.OnChangeActorEntity)
                .AddTo(_disposables);
            _actorEntity.RootScalar()
                .Subscribe(_footWaterSplashService.OnChangeRootScalar)
                .AddTo(_disposables);

            await UniTask.CompletedTask;
        }

        void ITickable.Tick()
        {
            _footWaterSplashService.OnTickAsync(_lifetime).Forget();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}


