using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class WaterRippleSettings : MonoBehaviour
    {
        public Renderer WaterRenderer => _waterRenderer;
        [SerializeField] Renderer _waterRenderer;

        public float WaterRippleAmp => _amp;
        [SerializeField] float _amp = 0.05f;
        public float WaterRippleInterval => _interval;
        [SerializeField] float _interval = 0.2f;

        void Awake()
        {
            Assert.IsNotNull(WaterRenderer);
        }
    }
}