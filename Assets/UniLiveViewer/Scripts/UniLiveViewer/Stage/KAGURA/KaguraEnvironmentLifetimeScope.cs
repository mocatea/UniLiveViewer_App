using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Kagura
{
    public class KaguraEnvironmentLifetimeScope : LifetimeScope
    {
        [SerializeField] KaguraEnvironmentSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_settings);
            builder.Register<KaguraEnvironmentService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<KaguraEnvironmentPresenter>();
        }
    }
}
