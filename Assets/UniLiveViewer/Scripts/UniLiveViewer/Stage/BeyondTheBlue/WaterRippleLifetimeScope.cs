using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class WaterRippleLifetimeScope : LifetimeScope
    {
        [SerializeField] WaterRippleSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_settings);
            builder.Register<WaterRippleService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<WaterRipplePresenter>();
        }
    }
}
