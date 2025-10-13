using UniLiveViewer.OVRCustom;
using UnityEngine;

namespace UniLiveViewer.Player.HandState
{
    public class GrabbedItemState : IHandState
    {
        readonly HandStateMachine _handStateMachine;

        public GrabbedItemState(HandStateMachine handStateMachine)
        {
            _handStateMachine = handStateMachine;
        }

        void IHandState.Enter()
        {
            // 特になし
        }

        void IHandState.OnSelectorChangeEnabled()
        {
            // 特になし
        }

        void IHandState.OnGrabBegin(OVRGrabbableCustom grabbable, bool isSummonCircle)
        {
            Debug.LogError("既にアイテムを握っている状態で、再度握り直したらおかしい");
        }

        void IHandState.OnGrabEnd(bool isSummonCircle)
        {
            if (isSummonCircle)
            {
                _handStateMachine.ChangeState(PlayerHandState.SUMMONCIRCLE);
            }
            else
            {
                _handStateMachine.ChangeState(PlayerHandState.DEFAULT);
            }
        }

        void IHandState.Exit()
        {
            // 特になし
        }
    }
}