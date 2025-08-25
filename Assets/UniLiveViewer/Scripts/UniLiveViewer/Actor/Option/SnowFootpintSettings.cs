using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Actor.Option
{
    public class SnowFootpintSettings : MonoBehaviour
    {
        // PEPlayer
        public Transform SnowFootPointPrefab => _snowFootPointPrefab;
        [SerializeField] Transform _snowFootPointPrefab;

        private void Awake()
        {
            Assert.IsNotNull(_snowFootPointPrefab);
        }
    }
}

