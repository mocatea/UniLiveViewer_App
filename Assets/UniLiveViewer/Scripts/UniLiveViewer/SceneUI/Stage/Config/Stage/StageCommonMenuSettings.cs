using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageCommonMenuSettings : MonoBehaviour
    {
        public SliderGrabController LightIntensitySlider => _lightIntensitySlider;
        [SerializeField] SliderGrabController _lightIntensitySlider;

        public SliderGrabController LightRotationSlider => _lightRotationSlider;
        [SerializeField] SliderGrabController _lightRotationSlider;

        public TextMesh LightIntensityText => _lightIntensityText;
        [SerializeField] TextMesh _lightIntensityText;

        public TextMesh LightRotationText => _lightRotationText;
        [SerializeField] TextMesh _lightRotationText;

        void Awake()
        {
            Assert.IsNotNull(_lightIntensitySlider);
            Assert.IsNotNull(_lightRotationSlider);
            Assert.IsNotNull(_lightIntensityText);
            Assert.IsNotNull(_lightRotationText);
        }
    }
}