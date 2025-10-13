using UniLiveViewer.Actor;
using UniLiveViewer.ValueObject;
using UnityEngine;

namespace UniLiveViewer.MessagePipe
{
    public class ActorStateMessage
    {
        public InstanceId InstanceId => _instanceId;
        InstanceId _instanceId;

        public ActorState State => _state;
        ActorState _state;
        public Transform OverrideTarget => _overrideTarget;
        Transform _overrideTarget;

        public ActorStateMessage(InstanceId instanceId, ActorState state, Transform overrideTarget)
        {
            _instanceId = instanceId;
            _state = state;
            _overrideTarget = overrideTarget;
        }
    }
}