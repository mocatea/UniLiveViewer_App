using UnityEngine;
using UnityEngine.Assertions;

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
        [SerializeField] Button_Base _ledButton;

        public TextMesh[] Texts => _texts;
        [SerializeField] TextMesh[] _texts = new TextMesh[4];

        void Awake()
        {
            Assert.IsNotNull(_particleButtons);
            Assert.IsNotNull(_wormHolleButtons);
            Assert.IsNotNull(_skyBoxButtons);
            Assert.IsNotNull(_ledButton);
            Assert.IsNotNull(_texts);

            for (int i = 0; i < _particleButtons.Length; i++)
            {
                Assert.IsNotNull(_particleButtons[i]);
            }
            for (int i = 0; i < _wormHolleButtons.Length; i++)
            {
                Assert.IsNotNull(_wormHolleButtons[i]);
            }
            for (int i = 0; i < _skyBoxButtons.Length; i++)
            {
                Assert.IsNotNull(_skyBoxButtons[i]);
            }
            for (int i = 0; i < _texts.Length; i++)
            {
                Assert.IsNotNull(_texts[i]);
            }
        }
    }
}