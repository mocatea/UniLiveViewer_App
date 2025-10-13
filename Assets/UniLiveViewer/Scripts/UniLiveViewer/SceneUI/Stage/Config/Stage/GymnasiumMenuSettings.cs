using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class GymnasiumMenuSettings : MonoBehaviour
    {
        public Button_Base[] SpotLightButtons => _spotLightButtons;
        [SerializeField] Button_Base[] _spotLightButtons = new Button_Base[2];

        public Button_Base LightColorButton => _lightColorButton;
        [SerializeField] Button_Base _lightColorButton;

        public TextMesh[] Texts => _texts;
        [SerializeField] TextMesh[] _texts = new TextMesh[1];

        void Awake()
        {
            Assert.IsNotNull(_spotLightButtons);
            Assert.IsNotNull(_lightColorButton);
            Assert.IsNotNull(_texts);

            for (int i = 0; i < _spotLightButtons.Length; i++)
            {
                Assert.IsNotNull(_spotLightButtons[i]);
            }
            for (int i = 0; i < _texts.Length; i++)
            {
                Assert.IsNotNull(_texts[i]);
            }
        }
    }
}