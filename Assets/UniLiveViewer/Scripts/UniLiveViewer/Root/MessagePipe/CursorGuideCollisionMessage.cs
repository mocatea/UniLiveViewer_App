using UniLiveViewer.Actor;
using UniLiveViewer.ValueObject;

namespace UniLiveViewer.MessagePipe
{
    public class CursorGuideCollisionMessage
    {
        public CursorGuideCollisions Collision => _collision;
        readonly CursorGuideCollisions _collision;

        public InstanceId InstanceId => _instanceId;
        readonly InstanceId _instanceId;

        public CursorGuideCollisionMessage(CursorGuideCollisions collision, InstanceId instanceId)
        {
            _collision = collision;
            _instanceId = instanceId;
        }
    }
}