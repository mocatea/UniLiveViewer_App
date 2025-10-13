using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.Actor.Expression;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.ValueObject;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Animation
{
    public class AnimationPresenter : IAsyncStartable, ILateTickable, IDisposable
    {
        readonly InstanceId _instanceId;
        readonly ISubscriber<AllActorOperationMessage> _allSubscriber;
        readonly AnimationService _animationService;
        readonly ExpressionService _expressionService;
        readonly IActorEntity _actorEntity;
        readonly ISubscriber<ActorAnimationMessage> _subscriber;
        readonly PresetResourceData _presetResourceData;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public AnimationPresenter(
            InstanceId instanceId,
            ISubscriber<AllActorOperationMessage> allSubscriber,
            AnimationService animationService,
            ExpressionService expressionService,
            IActorEntity actorEntity,
            ISubscriber<ActorAnimationMessage> subscriber,
            PresetResourceData presetResourceData)
        {
            _instanceId = instanceId;
            _allSubscriber = allSubscriber;
            _animationService = animationService;
            _expressionService = expressionService;
            _actorEntity = actorEntity;
            _subscriber = subscriber;
            _presetResourceData = presetResourceData;
        }

        UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _actorEntity.ActorEntity()
                .Subscribe(_animationService.OnChangeAnimator)
                .AddTo(_disposables);

            _actorEntity.ActorState()
                .Subscribe(async x => 
                {
                    if (x == ActorState.MINIATURE)
                    {
                        if (_animationService.IsPlaying()) return;
                        await _animationService.OnChangeScale(cancellation);
                    }
                    else if (x == ActorState.HOLD)
                    {
                        _animationService.Stop();
                    }
                    else if (x == ActorState.FIELD)
                    {
                        await _animationService.OnChangeScale(cancellation);
                    }
                }
                ).AddTo(_disposables);

            _subscriber
                .Subscribe(async x =>
                {
                    if (_instanceId != x.InstanceId) return;
                    await _animationService.SetAnimationAsync(x.Mode, x.AnimationIndex, x.IsReverse, cancellation);

                    DanceInfoData danceInfoData = null;
                    if (x.Mode == Menu.CurrentMode.PRESET)
                    {
                        danceInfoData = _presetResourceData.DanceInfoData[x.AnimationIndex];
                    }
                    else if (x.Mode == Menu.CurrentMode.CUSTOM)
                    {
                        danceInfoData = _presetResourceData.VMDDanceInfoData;
                    }
                    _expressionService.OnChangeAnimation(x.Mode, danceInfoData);

                }).AddTo(_disposables);

            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorState != ActorState.NULL) return;
                    if (x.ActorCommand == ActorCommand.TIMELINE_PLAY)
                    {
                        _animationService.OnPlayTimeline();
                    }
                    else if (x.ActorCommand == ActorCommand.TIMELINE_PAUSE)
                    {
                        _animationService.OnStopTimeline();
                    }
                    else if (x.ActorCommand == ActorCommand.TIMELINE_STOP)
                    {
                        _animationService.OnStopTimeline();
                        _animationService.TryVMDInitializePose();
                    }
                }).AddTo(_disposables);

            return UniTask.CompletedTask;
        }

        void ILateTickable.LateTick()
        {
            if (!_actorEntity.Active().Value) return;
            //掴まれている時以外は常時
            if (_actorEntity.ActorState().Value == ActorState.HOLD) return;
            _animationService.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
