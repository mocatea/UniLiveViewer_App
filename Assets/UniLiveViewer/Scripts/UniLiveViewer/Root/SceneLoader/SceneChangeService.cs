using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace UniLiveViewer.SceneLoader
{
    public enum SceneType
    {
        TITLE,
        CANDY_LIVE,
        KAGURA_LIVE,
        VIEWER,
        GYMNASIUM,
        BEYOND_THE_BLUE,
        SNOW_FIELD,
        FANTASY_VILLAGE,
    }

    public class SceneChangeService
    {
        //未使用
        public static string[] NameList = new string[]
        { "LiveScene", "KAGURAScene", "ViewerScene", "GymnasiumScene", "FantasyVillage" };

        public static SceneType GetSceneType => _current;
        static SceneType _current = SceneType.TITLE;

        public static string GetVisualName => _map[_current].GetVisualName();
        static Dictionary<SceneType, IScene> _map;

        public SceneChangeService()
        {
        }

        public void Initialize()
        {
            _map = new Dictionary<SceneType, IScene>
            {
                { SceneType.TITLE, new TitleScene() },
                { SceneType.CANDY_LIVE, new CRSScene() },
                { SceneType.KAGURA_LIVE, new KaguraScene() },
                { SceneType.VIEWER, new ViewerScene() },
                { SceneType.GYMNASIUM, new GymnasiumScene() },
                { SceneType.BEYOND_THE_BLUE, new BeyondTheBlueScene() },
                { SceneType.SNOW_FIELD, new SnowFieldScene() },
                { SceneType.FANTASY_VILLAGE, new FantasyVillageScene() }
            };

            _current = (SceneType)FileReadAndWriteUtility.UserProfile.LastSceneSceneTypeNo;
            SystemInfo.Initialize(_current);
        }

        public async UniTask ChangePreviousScene(CancellationToken cancellation)
        {
            var nextScene = (SceneType)FileReadAndWriteUtility.UserProfile.LastSceneSceneTypeNo;
            await ChangeAsync(nextScene, cancellation);
        }

        public async UniTask ChangeAsync(SceneType nextSceneType, CancellationToken cancellation)
        {
            _current = nextSceneType;
            var nextScene = _map[nextSceneType];
            await nextScene.BeginAsync(cancellation);

            if(nextSceneType != SceneType.TITLE)
            {
                FileReadAndWriteUtility.UserProfile.LastSceneSceneTypeNo = (int)nextSceneType;
                FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);//完了したら更新

                SystemInfo.Initialize(nextSceneType);
            }
        }
    }
}
