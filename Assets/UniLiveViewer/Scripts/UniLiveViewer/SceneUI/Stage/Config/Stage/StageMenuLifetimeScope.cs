using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] StageMenuOnEnableHandler _onEnableHandler;

        [SerializeField] StageCommonMenuSettings _stageCommonSettings;
        [SerializeField] CandyLiveMenuSettings _candyLiveMenuSettings;
        [SerializeField] KaguraLiveMenuSettings _kaguraLiveMenuSettings;
        [SerializeField] ViewerMenuSettings _viewerMenuSettings;
        [SerializeField] GymnasiumMenuSettings _gymnasiumMenuSettings;
        [SerializeField] BeyondTheBlueMenuSettings _beyondTheBlueMenuSettings;
        [SerializeField] SnowFieldMenuSettings _snowFieldMenuSettings;
        [SerializeField] FantasyVillageMenuSettings _fantasyVillageMenuSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_onEnableHandler);
            builder.RegisterComponent(_stageCommonSettings);
            builder.Register<StageCommonMenuService>(Lifetime.Singleton);

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
            else if (SceneChangeService.GetSceneType == SceneType.BEYOND_THE_BLUE)
            {
                builder.RegisterComponent(_beyondTheBlueMenuSettings);
                builder.Register<IStageMenuService, BeyondTheBlueMenuServie>(Lifetime.Singleton);
            }
            else if (SceneChangeService.GetSceneType == SceneType.SNOW_FIELD)
            {
                builder.RegisterComponent(_snowFieldMenuSettings);
                builder.Register<IStageMenuService, SnowFieldMenuServie>(Lifetime.Singleton);
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
            _candyLiveMenuSettings.gameObject.SetActive(IsSceneMatch(SceneType.CANDY_LIVE));
            _kaguraLiveMenuSettings.gameObject.SetActive(IsSceneMatch(SceneType.KAGURA_LIVE));
            _viewerMenuSettings.gameObject.SetActive(IsSceneMatch(SceneType.VIEWER));
            _gymnasiumMenuSettings.gameObject.SetActive(IsSceneMatch(SceneType.GYMNASIUM));
            _beyondTheBlueMenuSettings.gameObject.SetActive(IsSceneMatch(SceneType.BEYOND_THE_BLUE));
            _snowFieldMenuSettings.gameObject.SetActive(IsSceneMatch(SceneType.SNOW_FIELD));
            _fantasyVillageMenuSettings.gameObject.SetActive(IsSceneMatch(SceneType.FANTASY_VILLAGE));
            // TODO: 構成見直すまでの繋ぎ
            bool IsSceneMatch(SceneType sceneType) => SceneChangeService.GetSceneType == sceneType;
        }
    }
}
