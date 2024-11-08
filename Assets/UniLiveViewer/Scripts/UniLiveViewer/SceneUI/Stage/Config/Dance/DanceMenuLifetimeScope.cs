﻿using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Dance
{
    public class DanceMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] DanceMenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);

            builder.Register<DanceMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<DanceMenuPresenter>();
        }
    }
}
