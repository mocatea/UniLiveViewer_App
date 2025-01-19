using UnityEngine;

namespace UniLiveViewer.Stage.Viewer
{
    public class ViewerEnvironmentService : MonoBehaviour
    {
        Transform _floorLED;
        BackGroundController _backGroundCon;

        void Start()
        {
            _floorLED = GameObject.FindGameObjectWithTag("FloorLED").transform;
            _backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();

            _floorLED.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_view_led);
        }

        public void OnClickParticle(int moveIndex)
        {
            if (_backGroundCon == null) return;
            _backGroundCon.SetParticle(moveIndex);
        }

        public void OnClickWormHole(int moveIndex)
        {
            if (_backGroundCon == null) return;
            _backGroundCon.SetWormHole(moveIndex);
        }

        public void OnClickSkyBox(int moveIndex)
        {
            if (_backGroundCon == null) return;
            _backGroundCon.SetCubemap(moveIndex);
        }

        public void OnClickFloorLED(bool isEnable)
        {
            if (_floorLED == null) return;

            _floorLED.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_view_led = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }
    }
}