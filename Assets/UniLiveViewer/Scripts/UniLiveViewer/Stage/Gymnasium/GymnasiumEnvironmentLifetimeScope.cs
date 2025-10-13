using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class GymnasiumEnvironmentLifetimeScope : LifetimeScope
    {
        [SerializeField] GymnasiumEnvironmentSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_settings);
            builder.Register<GymnasiumEnvironmentService>(Lifetime.Singleton);
            builder.Register<StageLightChangeService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GymnasiumEnvironmentPresenter>();
        }
    }
}
