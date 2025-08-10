using UniLiveViewer.OVRCustom;

namespace UniLiveViewer.Player.HandState
{
    public interface IHandState
    {
        void Enter();

        void OnSelectorChangeEnabled();

        void OnGrabBegin(OVRGrabbableCustom grabbable, bool isSummonCircle);
        void OnGrabEnd(bool isSummonCircle);

        void Exit();
    }
}