using UnityEngine;

namespace UniLiveViewer.Player.Graphics
{
    public interface IBloom
    {
        void Initialize(bool useBloom, float threshold, float intensity, float scatter, bool useTint, Color color);

        void SetActive(bool isEnable);

        void ChangeResolutionScale(float v);

        void ChangeIteration(int count);

        void ChangeThreshold(float v);

        void ChangeIntensity(float v);

        void ChangeScatter(float v);

        void ChangeUseTint(bool isEnable);

        void ChangeTint(Color color);

        bool IsActive();
    }
}