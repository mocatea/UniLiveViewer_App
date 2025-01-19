using UnityEngine;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class GymnasiumMenuSettings : MonoBehaviour
    {
        public Button_Base[] SpotLightButtons => _spotLightButtons;
        [SerializeField] Button_Base[] _spotLightButtons = new Button_Base[2];

        public Button_Base LightColorButton => _lightColorButton;
        [SerializeField] Button_Base _lightColorButton = new();

        public TextMesh[] Texts => _texts;
        [SerializeField] TextMesh[] _texts = new TextMesh[1];
    }
}