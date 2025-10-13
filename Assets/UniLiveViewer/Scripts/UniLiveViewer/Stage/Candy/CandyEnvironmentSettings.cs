using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Stage.Candy
{
    public class CandyEnvironmentSettings : MonoBehaviour
    {
        public readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        public Transform Particle => _particle;
        [SerializeField] Transform _particle;
        public Transform LaserGun => _laserGun;
        [SerializeField] Transform _laserGun;
        public MeshRenderer FloorMirror => _floorMirror;
        [SerializeField] MeshRenderer _floorMirror;
        public Transform SonicBoom => _sonicBoom;
        [SerializeField] Transform _sonicBoom;
        public Transform ManualUI => _manualUI;
        [SerializeField] Transform _manualUI;

        void Awake()
        {
            Assert.IsNotNull(_particle);
            Assert.IsNotNull(_laserGun);
            Assert.IsNotNull(_floorMirror);
            Assert.IsNotNull(_sonicBoom);
            Assert.IsNotNull(_manualUI);
        }

        void Start()
        {
            // EnvironmentLSはMenuLSの子であり、該当ページが開かれるまで初期化できない為
            // 苦渋の末ここで初期化している
            _particle.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_particle);
            _laserGun.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_laser);
            _floorMirror.material.SetFloat(SmoothnessId, FileReadAndWriteUtility.UserProfile.scene_crs_reflection ? 1 : 0);
            _sonicBoom.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_sonic);
            _manualUI.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_manual);
        }
    }
}