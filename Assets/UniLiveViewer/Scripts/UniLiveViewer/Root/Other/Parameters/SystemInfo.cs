using UniLiveViewer.SceneLoader;

namespace UniLiveViewer
{
    public static class SystemInfo
    {
        //public static UserProfile UserProfile { get; private set; }

        public static float soundVolume_SE = 0.3f;//SE音量
        public static OVRManager.FixedFoveatedRenderingLevel levelFFR = OVRManager.FixedFoveatedRenderingLevel.Medium;//中心窩レンダリング
        public static string folderPath_Persistent;//システム設定値など

        //召喚上限(Title/CRS/KAGURA/VIEW/GYM/BTB/Snow/VILLAGE)
        public static readonly int[] MAXCHARA_QUEST1 = { 0, 3, 3, 5, 3, 3, 3, 2 };
        public static readonly int[] MAXCHARA_QUEST2 = { 0, 4, 4, 5, 4, 4, 4, 3 };
        public static readonly int[] MAXCHARA_QUEST3 = { 0, 5, 5, 5, 5, 5, 5, 4 };
        public static readonly int[] MAXCHARA_EDITOR = { 0, 5, 5, 5, 5, 5, 5, 5 };

        /// <summary>
        /// フィールドに存在できる最大キャラ数
        /// </summary>
        public static int MaxFieldChara => _maxFieldChara;
        static int _maxFieldChara;

        public static int GetMaxFieldActor(SceneType sceneType) => _current[(int)sceneType];
        static int[] _current;

        public static void Initialize(SceneType sceneType)
        {
            var myPlatform = UnityEngine.SystemInfo.deviceName;
            if (myPlatform.Contains("Oculus") || myPlatform.Contains("Meta"))
            {
                if (myPlatform.Contains("3")) _current = MAXCHARA_QUEST3;
                else if (myPlatform.Contains("2")) _current = MAXCHARA_QUEST2;
                else if (myPlatform.Contains("Quest")) _current = MAXCHARA_QUEST1;
            }
            else
            {
                _current = MAXCHARA_EDITOR;
            }
            _maxFieldChara = _current[(int)sceneType];

            // SDK前提だがLinqまで識別できる
            //var type = OVRPlugin.GetSystemHeadsetType();
            //switch (type)
            //{
            //    case OVRPlugin.SystemHeadset.Oculus_Quest:
            //        _maxFieldChara = MAXCHARA_QUEST1[(int)sceneType];
            //        break;
            //    case OVRPlugin.SystemHeadset.Oculus_Link_Quest:
            //        _maxFieldChara = MAXCHARA_QUEST1[(int)sceneType];
            //        break;
            //    case OVRPlugin.SystemHeadset.Oculus_Quest_2:
            //        _maxFieldChara = MAXCHARA_QUEST2[(int)sceneType];
            //        break;
            //    case OVRPlugin.SystemHeadset.Oculus_Link_Quest_2:
            //        _maxFieldChara = MAXCHARA_QUEST2[(int)sceneType];
            //        break;
            //    //TODO: SDK更新しないと Quest3がない
            //    default:
            //        _maxFieldChara = MAXCHARA_EDITOR[(int)sceneType];
            //        break;
            //}
        }
    }
}
