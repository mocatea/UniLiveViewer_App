using UnityEngine;
using VContainer;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class BeyondTheBlueEnvironmentService
    {
        readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        readonly BeyondTheBlueEnvironmentSettings _settings;

        [Inject]
        public BeyondTheBlueEnvironmentService(BeyondTheBlueEnvironmentSettings settings)
        {
            _settings = settings;
        }

        public void OnClickPropSet(int index)
        {
            for (int i = 0; i < _settings.PropSets.Length; i++)
            {
                _settings.PropSets[i].gameObject.SetActive(i == index);
            }
        }

        public void OnClickGodRay(bool isEnable)
        {
            _settings.GodRay.gameObject.SetActive(isEnable);
        }

        public void OnChangeWaterColor(float v)
        {
            Debug.Log("色味が揃わないので一旦なし");
            return;

            foreach (var renderer in _settings.Waters)
            {
                var preColor = renderer.material.GetColor(BaseColorId);
                var nextColor = Color.HSVToRGB(v, 0.7f, 0.12f);
                nextColor.a = preColor.a;
                renderer.material.SetColor(BaseColorId, nextColor);
            }
        }
    }
}