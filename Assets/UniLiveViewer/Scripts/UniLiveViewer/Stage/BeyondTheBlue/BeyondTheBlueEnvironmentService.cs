using UnityEngine;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class BeyondTheBlueEnvironmentService : MonoBehaviour
    {
        readonly int EmissionId = Shader.PropertyToID("_Emission");

        Transform _godRay;
        MeshRenderer _waterMeshRenderer;

        void Start()
        {
            _godRay = GameObject.FindGameObjectWithTag("GodRay").transform;
            var go = GameObject.FindGameObjectWithTag("WaterLevel").transform;
            _waterMeshRenderer = go.GetComponent<MeshRenderer>();

            _godRay.gameObject.SetActive(true);
            OnChangeWaterColor(0.58f);//水色
        }

        public void OnClickGodRay(bool isEnable)
        {
            if (_godRay == null) return;
            _godRay.gameObject.SetActive(isEnable);
        }

        public void OnChangeWaterColor(float v)
        {
            if (_waterMeshRenderer == null) return;
            _waterMeshRenderer.material.SetColor(EmissionId, Color.HSVToRGB(v, 0.7f, 0.12f));
        }
    }
}