using UniLiveViewer.Actor;

namespace UniLiveViewer.MessagePipe
{
    /// <summary>
    /// ActorIdに依存せずtimeline側とmenu側の全員に通知
    /// アクターはアクティブ状態に関係なく受信
    /// </summary>
    public class AllActorOperationMessage
    {
        /// <summary>
        /// NULLの場合は全員
        /// </summary>
        public ActorState ActorState => _actorState;
        ActorState _actorState;

        public ActorCommand ActorCommand => _command;
        ActorCommand _command;

        /// <summary>
        /// ActorState.NULLは全員
        /// </summary>
        public AllActorOperationMessage(ActorState actorState, ActorCommand command)
        {
            _actorState = actorState;
            _command = command;
        }
    }
}