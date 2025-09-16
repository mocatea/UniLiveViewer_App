using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UniLiveViewer.Player.Graphics
{
    /// <summary>
    /// 独自軽量版、ScriptableRendererFeatureベース
    /// </summary>
    public class MobileBloom : IBloom
    {
        readonly MobileFriendlyBloomFeature _renderFeature;

        public MobileBloom(MobileFriendlyBloomFeature customBloomRenderFeature)
        {
            _renderFeature = customBloomRenderFeature;
        }

        void IBloom.Initialize(bool useBloom, float threshold, float intensity, float scatter, bool useTint, Color color)
        {
            var setting = _renderFeature.settings;
            _renderFeature.SetActive(useBloom);
            setting.threshold = threshold;
            setting.intensity = intensity;
            setting.scatter = scatter;
            setting.useTint = useTint;
            setting.tint = color;
        }

        void IBloom.SetActive(bool isEnable)
        {
            _renderFeature.SetActive(isEnable);
        }

        void IBloom.ChangeResolutionScale(float v)
        {
            _renderFeature.settings.resolutionScale = v;
        }

        void IBloom.ChangeIteration(int count)
        {
            _renderFeature.settings.iterations = count;
        }

        void IBloom.ChangeThreshold(float v)
        {
            _renderFeature.settings.threshold = v;
        }

        void IBloom.ChangeIntensity(float v)
        {
            _renderFeature.settings.intensity = v;
        }

        void IBloom.ChangeScatter(float v)
        {
            _renderFeature.settings.scatter = v;
        }

        void IBloom.ChangeUseTint(bool isEnable)
        {
            _renderFeature.settings.useTint = isEnable;
        }

        void IBloom.ChangeTint(Color color)
        {
            _renderFeature.settings.tint = color;
        }

        bool IBloom.IsActive() => _renderFeature.isActive;
    }
}