using System.IO;

namespace UniLiveViewer
{
    public enum FOLDERTYPE
    {
        CHARA,
        MOTION,
        BGM,
        SETTING
    }

    class PathsInfo
    {

#if UNITY_EDITOR
        static string folderPath_Custom = "D:/User/UniLiveViewer";
        static string folderPath_Download = "D:/User/Download";
#elif UNITY_ANDROID
        private static string folderPath_Custom = "/storage/emulated/0/UniLiveViewer";
        private static string folderPath_Download = "/storage/emulated/0/Download";
#endif

        static string folderPath_Persistent = UnityEngine.Application.persistentDataPath;
        //static string folderPath_Persistent = UnityEngine.Application.temporaryCachePath;

        //個別
        static string[] folderName = { "Chara", "Motion", "BGM", "Setting" };
        const string cachePath = "Cache";
        const string lipSyncPath = "Lip-sync";
        
        public static int folder_length = folderName.Length;

        //
        const string readme_ja = "readme_ja.txt";
        const string readme_en = "readme_en.txt";
        const string defect = "不具合・Defect.txt";
        const string json = "System.json";

        public static string GetFullPath(FOLDERTYPE type)
        {
            return Path.Combine(folderPath_Custom + "/", folderName[(int)type]);
        }

        public static string GetFullPath_ThumbnailCache()
        {
            return Path.Combine(folderPath_Custom + "/", folderName[(int)FOLDERTYPE.CHARA] + "/", cachePath);
        }

        public static string GetFullPath_LipSync()
        {
            return Path.Combine(folderPath_Custom + "/", folderName[(int)FOLDERTYPE.MOTION] + "/", lipSyncPath);
        }

        public static string GetFullPath_Download()
        {
            return folderPath_Download;
        }

        public static string GetFullPath_README(USE_LANGUAGE language)
        {
            if (language == USE_LANGUAGE.EN) return Path.Combine(folderPath_Custom + "/", readme_en);
            else if (language == USE_LANGUAGE.JP) return Path.Combine(folderPath_Custom + "/", readme_ja);
            else return "";
        }
        public static string GetFullPath_DEFECT()
        {
            return Path.Combine(folderPath_Custom + "/", defect);
        }

        public static string GetFullPath_JSON()
        {
            return Path.Combine(folderPath_Persistent + "/", json);
        }
    }
}