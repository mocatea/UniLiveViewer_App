using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace UniLiveViewer.Player
{
    public class GraphicsSettingsService
    {
        UniversalAdditionalCameraData _cameraData;
        bool _cachePostProcessing;
        Bloom _bloom;
        DepthOfField _depthOfField;
        Tonemapping _tonemapping;
        Vignette _vignette;
        UniversalRenderPipelineAsset _urpAsset;

        readonly Camera _camera;
        readonly VolumeProfile _volumeProfile;
        readonly Light _light;

        [Inject]
        public GraphicsSettingsService(Camera camera, VolumeProfile volumeProfile, Light light)
        {
            _camera = camera;
            _volumeProfile = volumeProfile;
            _light = light;
        }

        public void Initialize()
        {
            _cameraData = _camera.GetComponent<UniversalAdditionalCameraData>();
            _cachePostProcessing = _cameraData.renderPostProcessing;
            _cameraData.antialiasing = (AntialiasingMode)FileReadAndWriteUtility.UserProfile.Antialiasing;

            _urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            _urpAsset.msaaSampleCount = FileReadAndWriteUtility.UserProfile.MSAALevel;

            if (_volumeProfile.TryGet<Bloom>(out var bloom))
            {
                _bloom = bloom;
                _bloom.active = FileReadAndWriteUtility.UserProfile.IsBloom;
                _bloom.threshold.value = FileReadAndWriteUtility.UserProfile.BloomThreshold;
                _bloom.intensity.value = FileReadAndWriteUtility.UserProfile.BloomIntensity;
                _bloom.tint.overrideState = true;
                _bloom.tint.value = Color.HSVToRGB(0.65f, 0.55f, 1);//水色
            }
            if (_volumeProfile.TryGet<DepthOfField>(out var depthOfField))
            {
                _depthOfField = depthOfField;
                _depthOfField.active = FileReadAndWriteUtility.UserProfile.IsDepthOfField;
            }
            if (_volumeProfile.TryGet<Tonemapping>(out var tonemapping))
            {
                _tonemapping = tonemapping;
                _tonemapping.active = FileReadAndWriteUtility.UserProfile.IsTonemapping;
            }
            if (_volumeProfile.TryGet<Vignette>(out var vignette))
            {
                _vignette = vignette;
                _vignette.active = false;
            }
            IfNeededSwitchPostprocessing();
        }

        public void ChangeLightIntensity(float v)
        {
            _light.intensity = v;
        }

        public void ChangeAntialiasing(AntialiasingMode mode)
        {
            if (_cameraData == null) return;
            _cameraData.antialiasing = mode;
            FileReadAndWriteUtility.UserProfile.Antialiasing = (int)mode;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeMASS(MSAASamples value)
        {
            if (_urpAsset == null) return;
            _urpAsset.msaaSampleCount = (int)value;//有効値：0,2,4,8
            FileReadAndWriteUtility.UserProfile.MSAALevel = (int)value;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeRenderScale(float value)
        {
            if (_urpAsset == null) return;
            _urpAsset.renderScale = value;
            IfNeededSwitchPostprocessing();
        }

        public void ChangeBloom(bool isEnable)
        {
            _bloom.active = isEnable;
            FileReadAndWriteUtility.UserProfile.IsBloom = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeDepthOfField(bool isEnable)
        {
            _depthOfField.active = isEnable;
            FileReadAndWriteUtility.UserProfile.IsDepthOfField = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeTonemapping(bool isEnable)
        {
            _tonemapping.active = isEnable;
            FileReadAndWriteUtility.UserProfile.IsTonemapping = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeVignette(bool isEnable)
        {
            _vignette.active = isEnable;
            IfNeededSwitchPostprocessing();
        }

        public void ChangeBloomThreshold(float v)
        {
            _bloom.threshold.value = v;
            FileReadAndWriteUtility.UserProfile.BloomThreshold = v;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void ChangeBloomIntensity(float v)
        {
            _bloom.intensity.value = v;
            FileReadAndWriteUtility.UserProfile.BloomIntensity = v;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void ChangeBloomColor(float v)
        {
            //_bloom.tint = new ColorParameter(Color.HSVToRGB(v, 0.5f, 1), overrideState: true);何故か機能しない
            _bloom.tint.overrideState = true;
            _bloom.tint.value = Color.HSVToRGB(v, 0.55f, 1);
            // NOTE: 専用ピッカー作ったら保存するようにする
        }

        public void OnChangePassthrough(bool isEnablePassthrough)
        {
            if (isEnablePassthrough) ForceChangePostprocessing(false);
            else ResetPostProcessing();
        }

        void ForceChangePostprocessing(bool isEnable)
        {
            if (_cameraData == null) return;
            _cachePostProcessing = _cameraData.renderPostProcessing;
            _cameraData.renderPostProcessing = isEnable;
        }

        void ResetPostProcessing()
        {
            if (_cameraData == null) return;
            _cameraData.renderPostProcessing = _cachePostProcessing;
        }

        void IfNeededSwitchPostprocessing()
        {
            var isEnable = false;

            if (_cameraData.antialiasing != AntialiasingMode.None) isEnable = true;
            if (_bloom.active) isEnable = true;
            if (_depthOfField.active) isEnable = true;
            if (_tonemapping.active) isEnable = true;
            if (_vignette.active) isEnable = true;

            _cameraData.renderPostProcessing = isEnable;
        }
    }
}
