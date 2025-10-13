using System.Collections.Generic;
using UniLiveViewer.OVRCustom;
using UniRx;

namespace UniLiveViewer.Player.HandState
{
    /// <summary>
    /// どうせOVR捨てるのでテキトー
    /// </summary>
    public class HandStateMachine
    {
        readonly Dictionary<PlayerHandState, IHandState> _states;

        public IReadOnlyReactiveProperty<PlayerHandState> HandState => _handState;
        readonly ReactiveProperty<PlayerHandState> _handState;

        IHandState _current;

        public HandStateMachine()
        {
            _states = new()
        {
            { PlayerHandState.DEFAULT, new DefaultHandState(this) },
            { PlayerHandState.SUMMONCIRCLE, new SummonCircleState(this) },
            { PlayerHandState.GRABBED_CHARA, new GrabbedActorState(this) },
            { PlayerHandState.CHARA_ONCIRCLE, new ActorOnCircleState(this) },
            { PlayerHandState.GRABBED_ITEM, new GrabbedItemState(this) },
            { PlayerHandState.GRABBED_OTHER, new GrabbedOtherState(this) },
        };

            _current = _states[PlayerHandState.DEFAULT];
            _handState = new(PlayerHandState.DEFAULT);
            _current.Enter();
        }

        public void ChangeState(PlayerHandState next)
        {
            if (_handState.Value == next) return;

            _current.Exit();
            _current = _states[next];
            _handState.Value = next;
            _current.Enter();
        }

        public void OnSelectorChangeEnabled()
        {
            _current.OnSelectorChangeEnabled();
        }

        public void OnGrabBegin(OVRGrabbableCustom grabbable, bool isSummonCircle)
        {
            _current.OnGrabBegin(grabbable, isSummonCircle);
        }

        public void OnGrabEnd(bool isSummonCircle)
        {
            _current.OnGrabEnd(isSummonCircle);
        }
    }
}