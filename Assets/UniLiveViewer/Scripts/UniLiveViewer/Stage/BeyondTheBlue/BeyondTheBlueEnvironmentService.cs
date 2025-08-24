using UnityEngine;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class BeyondTheBlueEnvironmentService : MonoBehaviour
    {
        Transform _godRay;
        Transform _waterLevel;

        void Start()
        {
            _godRay = GameObject.FindGameObjectWithTag("GodRay").transform;
            _waterLevel = GameObject.FindGameObjectWithTag("WaterLevel").transform;

            _godRay.gameObject.SetActive(true);

            var next = _waterLevel.transform.position;
            next.y = 0.3f;
            _waterLevel.transform.position = next;
        }

        public void OnClickGodRay(bool isEnable)
        {
            if (_godRay == null) return;

            _godRay.gameObject.SetActive(isEnable);
        }

        public void OnChangeWaterLevel(float h)
        {
            var next = _waterLevel.transform.position;
            next.y = h;
            _waterLevel.transform.position = next;
        }
    }
}