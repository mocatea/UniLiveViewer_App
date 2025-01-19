using UnityEngine;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class FantasyVillageMenuSettings : MonoBehaviour
    {
        public Button_Base DirectionalLightButton => _directionalLightButton;
        [SerializeField] Button_Base _directionalLightButton = new();
    }
}