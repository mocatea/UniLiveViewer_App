using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class KaguraLiveMenuSettings : MonoBehaviour
    {
        public Button_Base ParticleButton => _particleButton;
        [SerializeField] Button_Base _particleButton;

        public Button_Base ReflectionButton => _reflectionButton;
        [SerializeField] Button_Base _reflectionButton;

        public Button_Base SeaWavesButton => _seaWavesButton;
        [SerializeField] Button_Base _seaWavesButton;

        public SliderGrabController FogSlider => _fogSlider;
        [SerializeField] SliderGrabController _fogSlider;

        void Awake()
        {
            Assert.IsNotNull(_particleButton);
            Assert.IsNotNull(_reflectionButton);
            Assert.IsNotNull(_seaWavesButton);
            Assert.IsNotNull(_fogSlider);
        }
    }
}