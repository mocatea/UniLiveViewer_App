using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Stage.Kagura
{
    public class KaguraEnvironmentSettings : MonoBehaviour
    {
        public Transform Particle => _particle;
        [SerializeField] Transform _particle;
        public Transform Reflection => _reflection;
        [SerializeField] Transform _reflection;
        public Transform WaterAnchor => _waterAnchor;
        [SerializeField] Transform _waterAnchor;

        void Awake()
        {
            Assert.IsNotNull(_particle);
            Assert.IsNotNull(_reflection);
            Assert.IsNotNull(_waterAnchor);
        }

        void Start()
        {
            // EnvironmentLSはMenuLSの子であり、該当ページが開かれるまで初期化できない為
            // 苦渋の末ここで初期化している
            _particle.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_particle);
            _reflection.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_sea);
            _waterAnchor.transform.GetChild(0).gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_reflection);
        }
    }
}