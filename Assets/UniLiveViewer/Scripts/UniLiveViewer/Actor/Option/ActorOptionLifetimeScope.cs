using UniLiveViewer.SceneLoader;
using UniLiveViewer.Timeline;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class ActorOptionLifetimeScope : LifetimeScope
    {
        [SerializeField] SnowFootPrintSettings _snowFootPrintSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<FakeShadowService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<FakeShadowPresenter>();

            builder.Register<GuideAnchorService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GuideAnchorPresenter>();

            if (SceneChangeService.GetSceneType == SceneType.SNOW_FIELD)
            {
                builder.RegisterComponent(_snowFootPrintSettings);
                builder.Register<SnowFootPrintService>(Lifetime.Singleton);
                builder.RegisterEntryPoint<SnowFootPrintPresenter>();
            }
        }
    }
}

