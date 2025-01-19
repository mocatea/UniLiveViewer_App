using UnityEngine;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class ViewerMenuSettings : MonoBehaviour
    {
        public Button_Base[] ParticleButtons => _particleButtons;
        [SerializeField] Button_Base[] _particleButtons = new Button_Base[2];

        public Button_Base[] WormHolleButtons => _wormHolleButtons;
        [SerializeField] Button_Base[] _wormHolleButtons = new Button_Base[2];

        public Button_Base[] SkyBoxButtons => _skyBoxButtons;
        [SerializeField] Button_Base[] _skyBoxButtons = new Button_Base[2];

        public Button_Base LedButton => _ledButton;
        [SerializeField] Button_Base _ledButton = new();

        public TextMesh[] Texts => texts;
        [SerializeField] TextMesh[] texts = new TextMesh[4];
    }
}