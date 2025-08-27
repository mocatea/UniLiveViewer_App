using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class FootWaterSplashPresenter : IAsyncStartable, ILateTickable, IDisposable
    {
        CancellationTokenSource _lifetime = new();

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
            _actorEntity.ActorEntity()
                .Subscribe(_footWaterSplashService.OnChangeActorEntity)
                .AddTo(_disposables);
            _actorEntity.RootScalar()
                .Subscribe(_footWaterSplashService.OnChangeRootScalar)
                .AddTo(_disposables);

            await UniTask.CompletedTask;
        }

        void ILateTickable.LateTick()
        {
            _footWaterSplashService.OnLateTickAsync(_lifetime.Token).Forget();
        }

        void IDisposable.Dispose()
        {
            _lifetime.Cancel();
            _disposables.Dispose();
        }
    }
}


