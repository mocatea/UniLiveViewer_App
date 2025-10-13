using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Viewer
{
    public class ViewerEnvironmentLifetimeScope : LifetimeScope
    {
        [SerializeField] ViewerEnvironmentSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_settings);
            builder.Register<ViewerEnvironmentService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ViewerEnvironmentPresenter>();
        }
    }
}
