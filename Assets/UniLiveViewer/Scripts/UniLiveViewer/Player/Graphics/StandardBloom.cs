using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UniLiveViewer.Player.Graphics
{
    /// <summary>
    /// URP標準、volumeコンポーネント
    /// </summary>
    public class StandardBloom : IBloom
    {
        readonly Bloom _bloom;

        public StandardBloom(Bloom bloom)
        {
            _bloom = bloom;
        }

        void IBloom.Initialize(bool useBloom, float threshold, float intensity, float scatter, bool useTint, Color color)
        {
            _bloom.active = useBloom;
            _bloom.threshold.value = threshold;
            _bloom.intensity.value = intensity;
            _bloom.scatter.value = scatter;
            _bloom.tint.overrideState = useTint;
            _bloom.tint.value = color;
        }

        void IBloom.SetActive(bool isEnable)
        {
            _bloom.active = isEnable;
        }

        void IBloom.ChangeResolutionScale(float v)
        {
            // ない
        }

        void IBloom.ChangeIteration(int count)
        {
            // ない
        }

        void IBloom.ChangeThreshold(float v)
        {
            _bloom.threshold.value = v;
        }

        void IBloom.ChangeIntensity(float v)
        {
            _bloom.intensity.value = v;
        }

        void IBloom.ChangeScatter(float v)
        {
            _bloom.scatter.value = v;
        }

        void IBloom.ChangeUseTint(bool isEnable)
        {
            _bloom.tint.overrideState = isEnable;
        }

        void IBloom.ChangeTint(Color color)
        {
            //_bloom.tint = new ColorParameter(Color.HSVToRGB(v, 0.5f, 1), overrideState: true);何故か機能しない
            _bloom.tint.value = color;
        }

        bool IBloom.IsActive() => _bloom.active;
    }
}