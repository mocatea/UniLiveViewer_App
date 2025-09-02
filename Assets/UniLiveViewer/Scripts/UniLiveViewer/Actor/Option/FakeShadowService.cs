using System;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor.Option
{
    public class FakeShadowService : IDisposable
    {
        QuasiShadowSetting _settings;
        QuasiShadowSetting.Preset _preset;
        IFakeShadow _fakeShadow;

        readonly Transform _parent;

        [Inject]
        public FakeShadowService(ActorOptionLifetimeScope actorOptionLifetimeScope)
        {
            _parent = actorOptionLifetimeScope.transform;
        }

        public void Setup(SHADOWTYPE shadowType, float userShadowScale, QuasiShadowSetting settings, int presetIndex)
        {
            _settings = settings;
            _preset = settings.Presets[presetIndex];

            _fakeShadow = SceneChangeService.GetSceneType == SceneType.BEYOND_THE_BLUE ?
                new DecalShadow() : new LegacyShadow();
            _fakeShadow.Setup(_parent, shadowType, settings.Presets[presetIndex], userShadowScale);
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            _fakeShadow.OnChangeActorEntity(actorEntity);
        }

        public void SetEnable(bool isEnable)
        {
            _fakeShadow.SetEnable(isEnable);
        }

        public void OnChangeRootScalar(float rootScalar)
        {
            _fakeShadow.OnChangeRootScalar(rootScalar);
        }

        public void OnUpdateShadowSettings(SHADOWTYPE shadowType, float userShadowScale, QuasiShadowSetting settings, int presetIndex)
        {
            _settings = settings;
            _preset = settings.Presets[presetIndex];

            _fakeShadow.OnUpdateShadowType(shadowType, settings.Presets[presetIndex], userShadowScale);
        }

        public void OnTick()
        {
            _fakeShadow.OnUpdate(_preset, _settings);
        }

        public void Dispose()
        {
            _fakeShadow.Dispose();
        }
    }
}
