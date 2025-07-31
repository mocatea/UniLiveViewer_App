using UnityEngine;

namespace UniLiveViewer.Menu.Config.Actor
{
    public class ActorMenuSettings : MonoBehaviour
    {
        public SliderGrabController InitialActorSizeSlider => _initialActorSizeSlider;
        [SerializeField] SliderGrabController _initialActorSizeSlider;

        public TextMesh InitialActorSizeText => _initialActorSizeText;
        [SerializeField] TextMesh _initialActorSizeText;

        public Button_Base FallingShadowLButton => _fallingShadowLButton;
        [SerializeField] Button_Base _fallingShadowLButton;

        public Button_Base FallingShadowRButton => _fallingShadowRButton;
        [SerializeField] Button_Base _fallingShadowRButton;

        public SliderGrabController FallingShadowSlider => _fallingShadowSlider;
        [SerializeField] SliderGrabController _fallingShadowSlider;

        public TextMesh FallingShadowTypeText => _fallingShadowTypeText;
        [SerializeField] TextMesh _fallingShadowTypeText;

        public TextMesh FallingShadowValueText => _fallingShadowValueText;
        [SerializeField] TextMesh _fallingShadowValueText;
    }
}
