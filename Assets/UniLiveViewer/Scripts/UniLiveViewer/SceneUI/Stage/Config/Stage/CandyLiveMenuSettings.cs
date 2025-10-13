using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class CandyLiveMenuSettings : MonoBehaviour
    {
        public Button_Base ParticleButton => _particleButton;
        [SerializeField] Button_Base _particleButton;
        public Button_Base LaserGunButton => _laserGunButton;
        [SerializeField] Button_Base _laserGunButton;

        public Button_Base ReflectionButton => _reflectionButton;
        [SerializeField] Button_Base _reflectionButton;

        public Button_Base SonicBoomButton => _sonicBoomButton;
        [SerializeField] Button_Base _sonicBoomButton;

        public Button_Base PlayManualButton => _playManualButton;
        [SerializeField] Button_Base _playManualButton;

        void Awake()
        {
            Assert.IsNotNull(_particleButton);
            Assert.IsNotNull(_laserGunButton);
            Assert.IsNotNull(_reflectionButton);
            Assert.IsNotNull(_sonicBoomButton);
            Assert.IsNotNull(_playManualButton);
        }
    }
}