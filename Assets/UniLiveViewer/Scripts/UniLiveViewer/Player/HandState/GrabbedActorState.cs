using UniLiveViewer.OVRCustom;
using UnityEngine;

namespace UniLiveViewer.Player.HandState
{
    public class GrabbedActorState : IHandState
    {
        readonly HandStateMachine _handStateMachine;

        public GrabbedActorState(HandStateMachine handStateMachine)
        {
            _handStateMachine = handStateMachine;
        }

        void IHandState.Enter()
        {
            // 特になし
        }

        void IHandState.OnSelectorChangeEnabled()
        {
            // 掴んでいるアクターを召喚陣上に移す
            _handStateMachine.ChangeState(PlayerHandState.CHARA_ONCIRCLE);
        }

        void IHandState.OnGrabBegin(OVRGrabbableCustom grabbable, bool isSummonCircle)
        {
            Debug.LogError("既にアクターを握っている状態で、再度握り直したらおかしい");
        }

        void IHandState.OnGrabEnd(bool isSummonCircle)
        {
            // アクターを離す
            _handStateMachine.ChangeState(PlayerHandState.DEFAULT);
        }

        void IHandState.Exit()
        {
            // 特になし
        }
    }
}