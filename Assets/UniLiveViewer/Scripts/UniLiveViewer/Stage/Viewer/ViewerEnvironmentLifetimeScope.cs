using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Viewer
{
    [RequireComponent(typeof(ViewerEnvironmentService))]
    public class ViewerEnvironmentLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(GetComponent<ViewerEnvironmentService>());

            builder.RegisterEntryPoint<ViewerEnvironmentPresenter>();
        }
    }
}
