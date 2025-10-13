using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class GymnasiumEnvironmentSettings : MonoBehaviour
    {
        public Transform[] Lights => _lights;
        [SerializeField] Transform[] _lights;

        public IStageLight[] Stagelights => _stagelights;
        IStageLight[] _stagelights;

        void Awake()
        {
            Assert.IsNotNull(_lights);

            foreach (var light in _lights)
            {
                Assert.IsNotNull(light);
            }

            _stagelights = _lights
                .Select(t => t.GetComponent<IStageLight>())
                .Where(stageLight => stageLight != null)
                .ToArray();
        }

        void Start()
        {
            // EnvironmentLSはMenuLSの子であり、該当ページが開かれるまで初期化できない為
            // 苦渋の末ここで初期化している

            var currntIndex = StageEnums.StageLightDefaultIndex;
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].gameObject.SetActive(i == currntIndex);
            }

            _stagelights[currntIndex].ChangeCount(0);

            var isWhite = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;
            _stagelights[currntIndex].ChangeColor(isWhite);
        }
    }
}