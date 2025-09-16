using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Stage
{
    public class StageLightingService
    {
        public StageLightingService()
        {

        }

        public void Verify()
        {
            // シーン上に有効なlightがあれば未アタッチでも設定されてしまう模様
            Assert.IsNotNull(RenderSettings.sun,"アクティブなlightがシーン上に存在しません、Lighting画面にも明示的に設定すること");
        }

        public void ChangeLightColor(Color color)
        {
            RenderSettings.sun.color = color;
        }

        public void ChangeLightIntensity(float v)
        {
            RenderSettings.sun.intensity = v;
        }

        public void ChangeLightRotation(float yow)
        {
            var eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
            eulerAngles.y = yow;
            RenderSettings.sun.transform.eulerAngles = eulerAngles;
        }

        public void ChangeFogDensity(float v)
        {
            RenderSettings.fogDensity = v;
        }

        public float LightIntensity => RenderSettings.sun.intensity;
        public float LightRotationYow => RenderSettings.sun.transform.eulerAngles.y;
    }
}