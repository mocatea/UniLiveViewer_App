using System.Collections;
using System.Collections.Generic;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace UniLiveViewer
{
    public static class SystemInfo
    {
        //public static UserProfile UserProfile { get; private set; }

        public static float soundVolume_SE = 0.3f;//SE音量
        public static OVRManager.FixedFoveatedRenderingLevel levelFFR = OVRManager.FixedFoveatedRenderingLevel.Medium;//中心窩レンダリング
        public static string folderPath_Persistent;//システム設定値など

        //召喚上限(Title/CRS/KAGURA/VIEW/GYM/BTB/Snow/VILLAGE)
        static readonly int[] MAXACTOR_QUEST1 = { 0, 3, 3, 5, 3, 1, 0, 0 };
        static readonly int[] MAXACTOR_QUEST2 = { 0, 4, 4, 5, 4, 1, 0, 0 };
        static readonly int[] MAXACTOR_QUEST3 = { 0, 5, 5, 5, 5, 2, 0, 0 };
        static readonly int[] MAXACTOR_EDITOR = { 0, 5, 5, 5, 5, 3, 3, 5 };

        static  Dictionary<DeviceType, int[]> _map;
        enum DeviceType
        {
            None = 0,
            Quest1,
            Quest2,
            Quest3,//Sも含まれる
            Editor,
        }

        /// <summary>
        /// フィールドに存在できる最大キャラ数
        /// </summary>
        public static int MaxFieldActor => _maxFieldActor;
        static int _maxFieldActor;

        public static int GetMaxFieldActor(SceneType sceneType) => _current[(int)sceneType];
        static DeviceType _deviceType = DeviceType.None;
        static int[] _current;

        public static void Initialize(SceneType sceneType)
        {
            _map = new()
            {
                { DeviceType.None,MAXACTOR_QUEST2 },// 例外
                { DeviceType.Quest1,MAXACTOR_QUEST1 },
                { DeviceType.Quest2,MAXACTOR_QUEST2 },
                { DeviceType.Quest3,MAXACTOR_QUEST3 },
                { DeviceType.Editor,MAXACTOR_EDITOR },
            };

            var myPlatform = UnityEngine.SystemInfo.deviceName;
            if (myPlatform.Contains("Oculus") || myPlatform.Contains("Meta"))
            {
                if (myPlatform.Contains("3")) _deviceType = DeviceType.Quest3;
                else if (myPlatform.Contains("2")) _deviceType = DeviceType.Quest2;
                else if (myPlatform.Contains("Quest")) _deviceType = DeviceType.Quest1;
            }
            else if(Application.isEditor)
            {
                _deviceType = DeviceType.Editor;
            }

            _current = _map[_deviceType];
            _maxFieldActor = _current[(int)sceneType];

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
