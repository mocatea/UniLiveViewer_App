using UniLiveViewer.OVRCustom;
using UnityEngine;

namespace UniLiveViewer.Player.HandState
{
    public class ActorOnCircleState : IHandState
    {
        readonly HandStateMachine _handStateMachine;

        public ActorOnCircleState(HandStateMachine handStateMachine)
        {
            _handStateMachine = handStateMachine;
        }

        void IHandState.Enter()
        {
            // 特になし
        }

        void IHandState.OnSelectorChangeEnabled()
        {
            // 召喚陣上のアクターを手元に戻す
            _handStateMachine.ChangeState(PlayerHandState.GRABBED_CHARA);
        }

        void IHandState.OnGrabBegin(OVRGrabbableCustom grabbable, bool isSummonCircle)
        {
            Debug.LogError("既にアクターを召喚陣上で握っている状態で、再度握り直したらおかしい");
        }

        void IHandState.OnGrabEnd(bool isSummonCircle)
        {
            // アクターを召喚、サークルだけの状態に
            _handStateMachine.ChangeState(PlayerHandState.SUMMONCIRCLE);
        }

        void IHandState.Exit()
        {
            // 特になし
        }
    }
}