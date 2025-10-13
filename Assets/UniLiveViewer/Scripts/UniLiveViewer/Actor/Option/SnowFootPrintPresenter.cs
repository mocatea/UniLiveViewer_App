using MessagePipe;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class SnowFootprintPresenter : IStartable, IDisposable
    {
        readonly IActorEntity _actorEntity;
        readonly SnowFootprintService _snowFootprintService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SnowFootprintPresenter(
            IActorEntity actorEntity,
            SnowFootprintService snowFootprintService)
        {
            _actorEntity = actorEntity;
            _snowFootprintService = snowFootprintService;
        }

        void IStartable.Start()
        {
            _actorEntity.ActorEntity()
                .Subscribe(_snowFootprintService.OnChangeActorEntity)
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}


