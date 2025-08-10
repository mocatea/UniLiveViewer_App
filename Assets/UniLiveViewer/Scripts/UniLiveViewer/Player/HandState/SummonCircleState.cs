using UniLiveViewer.OVRCustom;

namespace UniLiveViewer.Player.HandState
{
    public class SummonCircleState : IHandState
    {
        readonly HandStateMachine _handStateMachine;

        public SummonCircleState(HandStateMachine handStateMachine)
        {
            _handStateMachine = handStateMachine;
        }

        void IHandState.Enter()
        {
            // 特になし
        }

        void IHandState.OnSelectorChangeEnabled()
        {
            // 召喚陣を消す
            _handStateMachine.ChangeState(PlayerHandState.DEFAULT);
        }

        void IHandState.OnGrabBegin(OVRGrabbableCustom grabbable, bool isSummonCircle)
        {
            if (grabbable == null)
            {
                // 何も握っていないので魔法陣で削除の可能性
                return;
            }

            var isActor = grabbable.CompareTag(Constants.TagGrabChara);
            if (isActor)
            {
                //召喚陣の上に乗せる
                _handStateMachine.ChangeState(PlayerHandState.CHARA_ONCIRCLE);
            }
        }

        void IHandState.OnGrabEnd(bool isSummonCircle)
        {
            if (!isSummonCircle)
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