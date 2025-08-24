using MessagePipe;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class SnowFootPrintPresenter : IStartable, IDisposable
    {
        readonly IActorEntity _actorEntity;
        readonly SnowFootPrintService _snowFootPrintService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SnowFootPrintPresenter(
            IActorEntity actorEntity,
            SnowFootPrintService snowFootPrintService)
        {
            _actorEntity = actorEntity;
            _snowFootPrintService = snowFootPrintService;
        }

        void IStartable.Start()
        {
            _actorEntity.ActorEntity()
                .Subscribe(_snowFootPrintService.OnChangeActorEntity)
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}


