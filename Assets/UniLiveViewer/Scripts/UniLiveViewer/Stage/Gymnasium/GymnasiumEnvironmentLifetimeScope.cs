using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Gymnasium
{
    [RequireComponent(typeof(StageLightChangeService))]
    public class GymnasiumEnvironmentLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GymnasiumEnvironmentService>(Lifetime.Singleton);
            builder.RegisterComponent(GetComponent<StageLightChangeService>());

            builder.RegisterEntryPoint<GymnasiumEnvironmentPresenter>();
        }
    }
}
