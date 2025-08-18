using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.Stage;
using UniLiveViewer.ValueObject;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    public class ActorEntityPresenter : IAsyncStartable, ITickable, IDisposable
    {
        readonly ISubscriber<AllActorOperationMessage> _allSubscriber;
        readonly ISubscriber<ActorOperationMessage> _subscriber;
        readonly ISubscriber<ActorStateMessage> _statePublisher;
        readonly ISubscriber<ActorResizeMessage> _resizeSubscriber;
        readonly IActorEntity _actorEntity;
        readonly InstanceId _instanceId;
        readonly GeneratorPortalAnchor _firstParent;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public ActorEntityPresenter(
            ISubscriber<AllActorOperationMessage> allSubscriber,
            ISubscriber<ActorOperationMessage> subscriber,
            ISubscriber<ActorStateMessage> statePublisher,
            ISubscriber<ActorResizeMessage> resizeSubscriber,
            IActorEntity actorEntity,
            InstanceId instanceId,
            GeneratorPortalAnchor firstParent)
        {
            _allSubscriber = allSubscriber;
            _subscriber = subscriber;
            _statePublisher = statePublisher;
            _resizeSubscriber = resizeSubscriber;
            _actorEntity = actorEntity;
            _instanceId = instanceId;
            _firstParent = firstParent;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _resizeSubscriber
                .Subscribe(x =>
                {
                    if (x.InstanceId != _instanceId) return;
                    _actorEntity.AddRootScalar(x.AddScale);
                }).AddTo(_disposables);

            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorState != _actorEntity.ActorState().Value) return;
                    OnCommand(x.ActorCommand);
                }).AddTo(_disposables);
            _subscriber
                .Subscribe(x =>
                {
                    if (x.InstanceId != _instanceId) return;
                    OnCommand(x.ActorCommand);
                }).AddTo(_disposables);
            _statePublisher
                .Subscribe(x =>
                {
                    if (x.InstanceId != _instanceId) return;
                    _actorEntity.SetState(x.State, x.OverrideTarget);
                })
                .AddTo(_disposables);

            await _actorEntity.SetupAsync(_firstParent.transform, cancellation);
        }

        void OnCommand(ActorCommand command)
        {
            if (command == ActorCommand.ACTIVE)
            {
                _actorEntity.Activate(true);
            }
            if (command == ActorCommand.INACTIVE)
            {
                _actorEntity.Activate(false);
            }
            if (command == ActorCommand.DELETE)
            {
                _actorEntity.Delete();
            }
        }

        void ITickable.Tick()
        {
            if (!_actorEntity.Active().Value) return;
            _actorEntity.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
