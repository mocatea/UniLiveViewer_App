using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class BeyondTheBlueMenuSettings : MonoBehaviour
    {
        public Button_Base GodRayButton => _godRayButton;
        [SerializeField] Button_Base _godRayButton;

        public SliderGrabController WaterLevelSlider => _waterLevelSlider;
        [SerializeField] SliderGrabController _waterLevelSlider;

        public TextMesh[] Texts => _texts;
        [SerializeField] TextMesh[] _texts = new TextMesh[1];

        void Awake()
        {
            Assert.IsNotNull(_godRayButton);
            Assert.IsNotNull(_waterLevelSlider);
            Assert.IsNotNull(_texts);

            for (int i = 0; i < _texts.Length; i++)
            {
                Assert.IsNotNull(_texts[i]);
            }
        }
    }
}