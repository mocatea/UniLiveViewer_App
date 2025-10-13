using UniLiveViewer.SceneLoader;
using UniLiveViewer.SO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace UniLiveViewer.Player.Graphics
{
    public class GraphicsSettingsService
    {
        readonly int RimIntensityId = Shader.PropertyToID("_RimIntensity");

        UniversalAdditionalCameraData _cameraData;
        bool _cachePostProcessing;

        IBloom _bloom;
        DepthOfField _depthOfField;
        Tonemapping _tonemapping;
        Vignette _vignette;
        UniversalRenderPipelineAsset _urpAsset;

        readonly Camera _camera;
        readonly VolumeProfile _volumeProfile;
        readonly PlayerGraphicsSettings _graphicsSettings;
        readonly SceneInitialSettings _sceneInitialSettings;

        [Inject]
        public GraphicsSettingsService(Camera camera, VolumeProfile volumeProfile,
            PlayerGraphicsSettings graphicsSettings, SceneInitialSettings sceneInitialSettings)
        {
            _camera = camera;
            _volumeProfile = volumeProfile;
            _graphicsSettings = graphicsSettings;
            _sceneInitialSettings = sceneInitialSettings;
        }

        public void Initialize()
        {
            _cameraData = _camera.GetComponent<UniversalAdditionalCameraData>();
            _cachePostProcessing = _cameraData.renderPostProcessing;
            _cameraData.antialiasing = (AntialiasingMode)FileReadAndWriteUtility.UserProfile.Antialiasing;

            _urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (_urpAsset == null)
            {
                Debug.LogError("_urpAsset is null");
                return;
            }
            _urpAsset.msaaSampleCount = FileReadAndWriteUtility.UserProfile.MSAALevel;

            _urpAsset.supportsHDR = true;
            //_camera.allowHDR = true; //supportsHDRを切り替えればこちらも自動で切り替わる

            var sceneData = _sceneInitialSettings.GetSettingData(SceneChangeService.GetSceneType);

            if (_volumeProfile.TryGet<Bloom>(out var bloom))
            {
                /*bloom.active = false;//念のため無効化
                var useTint = false;
                // 軽量Bloomを使う
                _bloom = new MobileBloom(_graphicsSettings.CustomBloomRenderFeature);
                _bloom.Initialize(useTint, Color.HSVToRGB(0.65f, 0.55f, 1));//水色*/

                //_graphicsSettings.CustomBloomRenderFeature.SetActive(false);
                var bloomData = sceneData.Bloom;
                _bloom = new StandardBloom(bloom);
                _bloom.Initialize(
                    bloomData.UseBloom,
                    bloomData.Threshold,
                    bloomData.Intensity,
                    bloomData.Scatter,
                    bloomData.UseColor,
                    ToBloomColor(bloomData.Hue));
            }

            if (_volumeProfile.TryGet<DepthOfField>(out var depthOfField))
            {
                _depthOfField = depthOfField;
                _depthOfField.active = FileReadAndWriteUtility.UserProfile.IsDepthOfField;
            }
            if (_volumeProfile.TryGet<Tonemapping>(out var tonemapping))
            {
                _tonemapping = tonemapping;
                _tonemapping.active = sceneData.Tonemapping;
            }
            if (_volumeProfile.TryGet<Vignette>(out var vignette))
            {
                _vignette = vignette;
                _vignette.active = false;
            }
            ChangeOutline(1);

            IfNeededSwitchPostprocessing();
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
            _bloom.SetActive(isEnable);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeBloomResolutionScale(float v)
        {
            _bloom.ChangeResolutionScale(v);
        }

        public void ChangeBloomThreshold(float v)
        {
            _bloom.ChangeThreshold(v);
        }

        public void ChangeBloomIntensity(float v)
        {
            _bloom.ChangeIntensity(v);
        }

        public void ChangeBloomScatter(float v)
        {
            _bloom.ChangeScatter(v);
        }

        public void ChangeUseBloomColor(bool isEnable)
        {
            _bloom.ChangeUseTint(isEnable);
            IfNeededSwitchPostprocessing();
        }

        public void ChangeBloomColorHue(float hue)
        {
            _bloom.ChangeTint(ToBloomColor(hue));
            // MEMO: 専用ピッカー作ったら保存するようにする？
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
            IfNeededSwitchPostprocessing();
        }

        public void ChangeOutline(float value)
        {
            if (0 < value)
            {
                _graphicsSettings.OutlineRenderFeature.SetActive(true);
                _graphicsSettings.OutlineMat.SetFloat(RimIntensityId, value);
            }
            else _graphicsSettings.OutlineRenderFeature.SetActive(false);
        }

        public void ChangeVignette(bool isEnable)
        {
            _vignette.active = isEnable;
            IfNeededSwitchPostprocessing();
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

            if (_bloom.IsActive()) isEnable = true;
            if (_depthOfField.active) isEnable = true;
            if (_tonemapping.active) isEnable = true;
            if (_vignette.active) isEnable = true;

            _cameraData.renderPostProcessing = isEnable;
        }

        Color ToBloomColor(float hue) => Color.HSVToRGB(hue, 0.55f, 1);
    }
}
