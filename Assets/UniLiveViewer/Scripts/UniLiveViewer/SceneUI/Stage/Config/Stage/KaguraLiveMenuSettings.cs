using UnityEngine;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class KaguraLiveMenuSettings : MonoBehaviour
    {
        public Button_Base ParticleButton => _particleButton;
        [SerializeField] Button_Base _particleButton = new();

        public Button_Base ReflectionButton => _reflectionButton;
        [SerializeField] Button_Base _reflectionButton = new();

        public Button_Base SeaWavesButton => _seaWavesButton;
        [SerializeField] Button_Base _seaWavesButton = new();

        public SliderGrabController FogSlider => _fogSlider;
        [SerializeField] SliderGrabController _fogSlider;
    }
}