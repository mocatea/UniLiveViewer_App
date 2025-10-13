using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Actor.Option
{
    public class FootWaterSplashSettings : MonoBehaviour
    {
        // BigSplash
        public Transform WaterSplashPrefab => _waterSplashPrefab;
        [SerializeField] Transform _waterSplashPrefab;

        public float Distance => _distance;
        [SerializeField] float _distance = 0.002f;

        public float ReferenceWaterLevel => _referenceWaterLevel;
        [SerializeField] float _referenceWaterLevel = 0.2f;


        private void Awake()
        {
            Assert.IsNotNull(_waterSplashPrefab);
        }
    }
}

