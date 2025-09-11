using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageCommonMenuSettings : MonoBehaviour
    {
        public SliderGrabController LightIntensitySlider => _lightIntensitySlider;
        [SerializeField] SliderGrabController _lightIntensitySlider;

        public TextMesh LightIntensityText => _lightIntensityText;
        [SerializeField] TextMesh _lightIntensityText;

        void Awake()
        {
            Assert.IsNotNull(_lightIntensitySlider);
            Assert.IsNotNull(_lightIntensityText);
        }
    }
}