using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] StageMenuOnEnableHandler _onEnableHandler;

        [SerializeField] CandyLiveMenuSettings _candyLiveMenuSettings;
        [SerializeField] KaguraLiveMenuSettings _kaguraLiveMenuSettings;
        [SerializeField] ViewerMenuSettings _viewerMenuSettings;
        [SerializeField] GymnasiumMenuSettings _gymnasiumMenuSettings;
        [SerializeField] FantasyVillageMenuSettings _fantasyVillageMenuSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_onEnableHandler);

            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                builder.RegisterComponent(_candyLiveMenuSettings);
                builder.Register<IStageMenuService, CandyLiveMenuServie>(Lifetime.Singleton);
            }
            else if (SceneChangeService.GetSceneType == SceneType.KAGURA_LIVE)
            {
                builder.RegisterComponent(_kaguraLiveMenuSettings);
                builder.Register<IStageMenuService, KaguraLiveMenuServie>(Lifetime.Singleton);
            }
            else if (SceneChangeService.GetSceneType == SceneType.VIEWER)
            {
                builder.RegisterComponent(_viewerMenuSettings);
                builder.Register<IStageMenuService, ViewerMenuServie>(Lifetime.Singleton);
            }
            else if (SceneChangeService.GetSceneType == SceneType.GYMNASIUM)
            {
                builder.RegisterComponent(_gymnasiumMenuSettings);
                builder.Register<IStageMenuService, GymnasiumMenuServie>(Lifetime.Singleton);
            }
            else if (SceneChangeService.GetSceneType == SceneType.FANTASY_VILLAGE)
            {
                builder.RegisterComponent(_fantasyVillageMenuSettings);
                builder.Register<IStageMenuService, FantasyVillageMenuServie>(Lifetime.Singleton);
            }

            builder.RegisterEntryPoint<StageMenuPresenter>();
        }

        void Start()
        {
            // TODO: 一旦雑にまた考える
            _candyLiveMenuSettings.gameObject.SetActive(false);
            _kaguraLiveMenuSettings.gameObject.SetActive(false);
            _viewerMenuSettings.gameObject.SetActive(false);
            _gymnasiumMenuSettings.gameObject.SetActive(false);
            _fantasyVillageMenuSettings.gameObject.SetActive(false);

            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                _candyLiveMenuSettings.gameObject.SetActive(true);
            }
            else if (SceneChangeService.GetSceneType == SceneType.KAGURA_LIVE)
            {
                _kaguraLiveMenuSettings.gameObject.SetActive(true);
            }
            else if (SceneChangeService.GetSceneType == SceneType.VIEWER)
            {
                _viewerMenuSettings.gameObject.SetActive(true);
            }
            else if (SceneChangeService.GetSceneType == SceneType.GYMNASIUM)
            {
                _gymnasiumMenuSettings.gameObject.SetActive(true);
            }
            else if (SceneChangeService.GetSceneType == SceneType.FANTASY_VILLAGE)
            {
                _fantasyVillageMenuSettings.gameObject.SetActive(true);
            }
        }
    }
}
