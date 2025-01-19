using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Kagura
{
    [RequireComponent(typeof(KaguraEnvironmentService))]
    public class KaguraEnvironmentLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(GetComponent<KaguraEnvironmentService>());

            builder.RegisterEntryPoint<KaguraEnvironmentPresenter>();
        }
    }
}
