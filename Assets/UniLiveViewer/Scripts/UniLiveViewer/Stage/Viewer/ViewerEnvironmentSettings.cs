using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Stage.Viewer
{
    public class ViewerEnvironmentSettings : MonoBehaviour
    {
        public Transform FloorLED => _floorLED;
        [SerializeField] Transform _floorLED;
        public BackGroundController BackGroundCon => _backGroundCon;
        [SerializeField] BackGroundController _backGroundCon;

        void Awake()
        {
            Assert.IsNotNull(_floorLED);
            Assert.IsNotNull(_backGroundCon);
        }

        void Start()
        {
            // EnvironmentLSはMenuLSの子であり、該当ページが開かれるまで初期化できない為
            // 苦渋の末ここで初期化している
            _floorLED.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_view_led);
        }
    }
}