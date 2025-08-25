using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Actor.Option
{
    public class FootWaterSplashSettings : MonoBehaviour
    {
        // BigSplash
        public Transform WaterSplashPrefab => _waterSplashPrefab;
        [SerializeField] Transform _waterSplashPrefab;

        private void Awake()
        {
            Assert.IsNotNull(_waterSplashPrefab);
        }
    }
}

