using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.ValueObject;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class GuideAnchorPresenter : IStartable, ITickable, IDisposable
    {
        readonly ISubscriber<AllActorOptionMessage> _subscriber;
        readonly ISubscriber<CursorGuideCollisionMessage> _collisionSubscriber;
        readonly IActorEntity _actorEntity;
        readonly GuideAnchorService _guideAnchorService;
        readonly InstanceId _instanceId;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GuideAnchorPresenter(
            ISubscriber<AllActorOptionMessage> subscriber,
            ISubscriber<CursorGuideCollisionMessage> collisionSubscriber,
            IActorEntity actorEntity,
            GuideAnchorService guideAnchorService,
            InstanceId instanceId)
        {
            _subscriber = subscriber;
            _collisionSubscriber = collisionSubscriber;
            _actorEntity = actorEntity;
            _guideAnchorService = guideAnchorService;
            _instanceId = instanceId;
        }

        void IStartable.Start()
        {
            _actorEntity.ActorEntity()
                .Subscribe(_guideAnchorService.OnChangeActorEntity)
                .AddTo(_disposables);

            _subscriber
                .Subscribe(x =>
                {
                    if (x.ActorState == ActorState.NULL) return;
                    if (x.ActorState != _actorEntity.ActorState().Value) return;
                    _guideAnchorService.SetEnable(x.ActorCommand == ActorOptionCommand.GUIDE_ANCHOR_ENEBLE);
                }).AddTo(_disposables);

            _collisionSubscriber
                .Subscribe(x =>
                {
                    if (x.Collision == CursorGuideCollisions.NO_HIT)
                    {
                        _guideAnchorService.OnPointerExit();
                    }
                    else if (x.Collision == CursorGuideCollisions.HIT)
                    {
                        if(x.InstanceId == _instanceId)
                        {
                            _guideAnchorService.OnPointerEnter();
                        }
                        else
                        {
                            _guideAnchorService.OnPointerExit();
                        }
                    }
                }).AddTo(_disposables);

            _guideAnchorService.Setup();
        }

        void ITickable.Tick()
        {
            if (!_actorEntity.Active().Value) return;
            _guideAnchorService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
