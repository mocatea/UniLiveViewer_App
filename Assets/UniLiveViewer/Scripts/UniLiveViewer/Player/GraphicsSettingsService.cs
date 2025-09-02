using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace UniLiveViewer.Player
{
    public class GraphicsSettingsService
    {
        readonly int EdgeId = Shader.PropertyToID("_Edge");

        UniversalAdditionalCameraData _cameraData;
        bool _cachePostProcessing;
        Bloom _bloom;
        DepthOfField _depthOfField;
        Tonemapping _tonemapping;
        Vignette _vignette;
        UniversalRenderPipelineAsset _urpAsset;

        readonly Camera _camera;
        readonly VolumeProfile _volumeProfile;
        readonly PlayerGraphicsSettings _graphicsSettings;
        readonly Light _light;

        [Inject]
        public GraphicsSettingsService(Camera camera, VolumeProfile volumeProfile,
            PlayerGraphicsSettings graphicsSettings, Light light)
        {
            _camera = camera;
            _volumeProfile = volumeProfile;
            _graphicsSettings = graphicsSettings;
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
            ChangeOutline(0.3f);

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
            if (mode == AntialiasingMode.SubpixelMorphologicalAntiAliasing)
            {
                _cameraData.antialiasingQuality = AntialiasingQuality.Low;//SMAA専用設定
            }
            FileReadAndWriteUtility.UserProfile.Antialiasing = (int)mode;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeMSAA(MSAASamples value)
        {
            if (_urpAsset == null) return;
            if (value == MSAASamples.MSAA4x || value == MSAASamples.MSAA8x)
            {
                value = MSAASamples.MSAA2x;//上限とする
            }
            _urpAsset.msaaSampleCount = (int)value;
            FileReadAndWriteUtility.UserProfile.MSAALevel = (int)value;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeRenderScale(float value)
        {
            Debug.LogError("RenderScaleはQuestでは使用しない予定");// 負荷もあるがRendertexture異常、恐らくシーン再ロード必要
            return;
            if (_urpAsset == null) return;
            _urpAsset.renderScale = value;
            IfNeededSwitchPostprocessing();
        }

        public void ChangeOpaqueDownsampling(Downsampling sampling)
        {
            // URPverが古いのでパイプライン切り替え手段しかないが機能しなそうだった
            // 本来はUniversalRenderPipelineAssetかUniversalRendererDataにopaqueDownsamplingがあるらしい
            Debug.LogError("OpaqueDownsamplingはQuestでは使用しない予定");
            return;

            if (sampling == Downsampling.None)
            {
                GraphicsSettings.renderPipelineAsset = _graphicsSettings.UrpAssets[0];
            }
            else if (sampling == Downsampling._2xBilinear)
            {
                GraphicsSettings.renderPipelineAsset = _graphicsSettings.UrpAssets[1];
            }
            else if (sampling == Downsampling._4xBilinear)
            {
                GraphicsSettings.renderPipelineAsset = _graphicsSettings.UrpAssets[2];
            }
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

        public void ChangeOutline(float value)
        {
            if (0 < value)
            {
                _graphicsSettings.OutlineRender.SetActive(true);
                _graphicsSettings.OutlineMat.SetFloat(EdgeId, value);
            }
            else _graphicsSettings.OutlineRender.SetActive(false);
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
