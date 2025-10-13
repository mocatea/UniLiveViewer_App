using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class BeyondTheBlueEnvironmentLifetimeScope : LifetimeScope
    {
        [SerializeField] BeyondTheBlueEnvironmentSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_settings);
            builder.Register<BeyondTheBlueEnvironmentService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BeyondTheBlueEnvironmentPresenter>();
        }
    }
}
