using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    [RequireComponent(typeof(BeyondTheBlueEnvironmentService))]
    public class BeyondTheBlueEnvironmentLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(GetComponent<BeyondTheBlueEnvironmentService>());

            builder.RegisterEntryPoint<BeyondTheBlueEnvironmentPresenter>();
        }
    }
}
