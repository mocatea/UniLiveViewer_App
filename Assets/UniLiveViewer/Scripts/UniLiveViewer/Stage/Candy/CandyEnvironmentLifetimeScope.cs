using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Candy
{
    [RequireComponent(typeof(CandyEnvironmentService))]
    public class CandyEnvironmentLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(GetComponent<CandyEnvironmentService>());

            builder.RegisterEntryPoint<CandyEnvironmentPresenter>();
        }
    }
}
