using UniLiveViewer.SceneLoader;
using UnityEngine;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class CandyLiveMenuSettings : MonoBehaviour
    {
        public Button_Base ParticleButton => _particleButton;
        [SerializeField] Button_Base _particleButton = new();
        public Button_Base LaserGunButton => _laserGunButton;
        [SerializeField] Button_Base _laserGunButton = new();

        public Button_Base ReflectionButton => _reflectionButton;
        [SerializeField] Button_Base _reflectionButton = new();

        public Button_Base SonicBoomButton => _sonicBoomButton;
        [SerializeField] Button_Base _sonicBoomButton = new();

        public Button_Base PlayManualButton => _playManualButton;
        [SerializeField] Button_Base _playManualButton = new();
    }
}