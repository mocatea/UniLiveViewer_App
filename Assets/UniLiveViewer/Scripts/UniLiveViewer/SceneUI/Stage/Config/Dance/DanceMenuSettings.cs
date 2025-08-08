using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Dance
{
    public class DanceMenuSettings : MonoBehaviour
    {
        public Button_Base VMDSmoothButton => _vmdSmoothButton;
        [SerializeField] Button_Base _vmdSmoothButton;

        public SliderGrabController VMDScaleSlider => _vmdScaleSlider;
        [SerializeField] SliderGrabController _vmdScaleSlider;

        public TextMesh VMDScaleText => _vmdScaleText;
        [SerializeField] TextMesh _vmdScaleText;

        void Awake()
        {
            Assert.IsNotNull(_vmdSmoothButton);
            Assert.IsNotNull(_vmdScaleSlider);
            Assert.IsNotNull(_vmdScaleText);
        }
    }
}
