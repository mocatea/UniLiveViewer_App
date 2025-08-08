using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class FantasyVillageMenuSettings : MonoBehaviour
    {
        public Button_Base DirectionalLightButton => _directionalLightButton;
        [SerializeField] Button_Base _directionalLightButton;

        void Awake()
        {
            Assert.IsNotNull(_directionalLightButton);
        }
    }
}