using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.SceneSelect
{
    public class SceneSelectMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] SceneSelectMenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);

            builder.Register<SceneSelectMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SceneSelectMenuPresenter>();
        }
    }
}
