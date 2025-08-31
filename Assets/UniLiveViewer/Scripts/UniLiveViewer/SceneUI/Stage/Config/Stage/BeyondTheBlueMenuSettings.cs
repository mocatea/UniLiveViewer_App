using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class BeyondTheBlueMenuSettings : MonoBehaviour
    {
        public Button_Base[] PropSetButtons => _propSetButtons;
        [SerializeField] Button_Base[] _propSetButtons;

        public TextMesh PropSetText => _propSetText;
        [SerializeField] TextMesh _propSetText;

        public Button_Base GodRayButton => _godRayButton;
        [SerializeField] Button_Base _godRayButton;

        public SliderGrabController WaterColorSlider => _waterColorSlider;
        [SerializeField] SliderGrabController _waterColorSlider;

        void Awake()
        {
            Assert.IsNotNull(_propSetButtons);
            Assert.IsNotNull(_propSetText);
            Assert.IsNotNull(_godRayButton);
            Assert.IsNotNull(_waterColorSlider);

            foreach (var button in _propSetButtons)
            {
                Assert.IsNotNull(button);
            }
        }
    }
}