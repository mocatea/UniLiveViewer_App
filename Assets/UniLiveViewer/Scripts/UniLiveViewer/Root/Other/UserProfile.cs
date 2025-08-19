using UnityEngine.Rendering.Universal;

namespace UniLiveViewer
{
    public class UserProfile
    {
        public int LanguageCode = -1;//未設定-1,EN0,JP1
        public int LastSceneSceneTypeNo = 1;//SceneLoader.SceneType.CANDY_LIVE
        public float InitCharaSize = 1.1f;
        public float CharaShadowSize = 1.25f;
        public int CharaShadowType = 7;
        public bool IsSmoothVMD = false;
        public bool IsVRM10 = true;
        public float VMDScale = 0.750f;
        public bool TouchVibration = true;
        //public bool StepSE = true;//廃止

        public int Antialiasing = (int)AntialiasingMode.None;
        public bool IsBloom = true;
        public float BloomThreshold = 0.95f;
        public float BloomIntensity = 5.0f;
        public bool IsDepthOfField = false;
        public bool IsTonemapping = true;

        public float SoundMaster = 100;
        public float SoundBGM = 100;
        public float SoundSE = 100;
        public float SoundAmbient = 100;
        public float SoundFootSteps = 100;
        public float SoundSpectrumGain = 50;
        public float SoundSpectrumSmoothness = 50;

        public bool scene_crs_particle = true;
        public bool scene_crs_laser = true;
        public bool scene_crs_reflection = true;
        public bool scene_crs_sonic = true;
        public bool scene_crs_manual = true;

        public bool scene_kagura_particle = true;
        public bool scene_kagura_sea = true;
        public bool scene_kagura_reflection = true;

        public bool scene_view_led = true;

        public bool scene_gym_whitelight = true;

        public bool scene_fv_light = true;
    }
}