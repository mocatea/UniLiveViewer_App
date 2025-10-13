using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Candy
{
    public class CandyEnvironmentLifetimeScope : LifetimeScope
    {
        [SerializeField] CandyEnvironmentSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_settings);
            builder.Register<CandyEnvironmentService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CandyEnvironmentPresenter>();
        }
    }
}
