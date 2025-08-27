using UnityEngine;

namespace UniLiveViewer.Actor.Option
{
    public interface IFakeShadow
    {
        void Setup(Transform parent, SHADOWTYPE shadowType, QuasiShadowSetting.Preset preset, float userShadowScale);

        void OnChangeActorEntity(ActorEntity actorEntity);

        void SetEnable(bool isEnable);

        void OnChangeRootScalar(float rootScalar);

        void OnUpdateShadowType(SHADOWTYPE shadowType, QuasiShadowSetting.Preset preset);

        void OnUpdate(QuasiShadowSetting.Preset preset, QuasiShadowSetting setting);

        void Dispose();
    }
}