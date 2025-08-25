using UniLiveViewer.SceneLoader;
using UniLiveViewer.Timeline;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class ActorOptionLifetimeScope : LifetimeScope
    {
        [SerializeField] AudioSourceService _audioSourceService;
        [SerializeField] FootWaterSplashSettings _footWaterSplashSettings;
        [SerializeField] SnowFootpintSettings _snowFootprintSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<FakeShadowService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<FakeShadowPresenter>();

            builder.Register<GuideAnchorService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GuideAnchorPresenter>();

            builder.RegisterInstance(_audioSourceService);

            FootActionConfigure(builder);
        }

        void FootActionConfigure(IContainerBuilder builder)
        {
            builder.Register<FootstepService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<FootstepPresenter>(Lifetime.Singleton);

            if (SceneChangeService.GetSceneType == SceneType.BEYOND_THE_BLUE)
            {
                builder.RegisterComponent(_footWaterSplashSettings);
                builder.Register<FootWaterSplashService>(Lifetime.Singleton);
                builder.RegisterEntryPoint<FootWaterSplashPresenter>();
            }
            else if (SceneChangeService.GetSceneType == SceneType.SNOW_FIELD)
            {
                builder.RegisterComponent(_snowFootprintSettings);
                builder.Register<SnowFootprintService>(Lifetime.Singleton);
                builder.RegisterEntryPoint<SnowFootprintPresenter>();
            }
        }
    }
}

