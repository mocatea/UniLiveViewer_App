using UniLiveViewer.OVRCustom;

namespace UniLiveViewer.Player.HandState
{
    public class DefaultHandState : IHandState
    {
        readonly HandStateMachine _handStateMachine;

        public DefaultHandState(HandStateMachine handStateMachine)
        {
            _handStateMachine = handStateMachine;
        }

        void IHandState.Enter()
        {
            // 特になし
        }

        void IHandState.OnSelectorChangeEnabled()
        {
            // 召喚陣を出現
            _handStateMachine.ChangeState(PlayerHandState.SUMMONCIRCLE);
        }

        void IHandState.OnGrabBegin(OVRGrabbableCustom grabbable, bool isSummonCircle)
        {
            if (grabbable == null) return;

            var isActor = grabbable.CompareTag(Constants.TagGrabChara);
            if (isActor)
            {
                if (isSummonCircle)
                {
                    //召喚陣の上に乗せる
                    _handStateMachine.ChangeState(PlayerHandState.CHARA_ONCIRCLE);
                }
                else
                {
                    //手に持たせる
                    _handStateMachine.ChangeState(PlayerHandState.GRABBED_CHARA);
                }
            }
            else
            {
                if (grabbable.IsBothHandsGrab)
                {
                    // 両手掴みはアイテム...
                    // TODO: ここ仕様から練り直したい
                    _handStateMachine.ChangeState(PlayerHandState.GRABBED_ITEM);
                }
                else
                {
                    _handStateMachine.ChangeState(PlayerHandState.GRABBED_OTHER);
                }
            }
        }

        void IHandState.OnGrabEnd(bool isSummonCircle)
        {
            if (isSummonCircle)
            {
                _handStateMachine.ChangeState(PlayerHandState.SUMMONCIRCLE);
            }
        }

        void IHandState.Exit()
        {
            // 特になし
        }
    }
}