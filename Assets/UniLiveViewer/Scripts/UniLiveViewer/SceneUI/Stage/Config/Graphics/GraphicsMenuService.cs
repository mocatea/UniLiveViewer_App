using System;
using UniLiveViewer.Player;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;
using static UniLiveViewer.Player.GraphicsExtensions;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuService : IDisposable
    {
        public IReadOnlyReactiveProperty<float> LightIntensity => _lightIntensity;
        readonly ReactiveProperty<float> _lightIntensity = new(1);

        public IReadOnlyReactiveProperty<AntialiasingMode> AntialiasingModeValue => _antialiasingMode;
        readonly ReactiveProperty<AntialiasingMode> _antialiasingMode = new((AntialiasingMode)FileReadAndWriteUtility.UserProfile.Antialiasing);

        public IReadOnlyReactiveProperty<MSAASamples> MSAASamplesValue => _msaaSamples;
        readonly ReactiveProperty<MSAASamples> _msaaSamples = new((MSAASamples)FileReadAndWriteUtility.UserProfile.MSAALevel);
        readonly MSAASamples[] _msaaOptions = new MSAASamples[] { MSAASamples.None, MSAASamples.MSAA2x, MSAASamples.MSAA4x };// なんで数値飛んでるのメンド

        public IReadOnlyReactiveProperty<float> RenderScale => _renderScale;
        readonly ReactiveProperty<float> _renderScale = new(1);

        public IReadOnlyReactiveProperty<Downsampling> OpaqueDownsampling => _opaqueDownsampling;
        readonly ReactiveProperty<Downsampling> _opaqueDownsampling = new(Downsampling._2xBilinear);

        public IReadOnlyReactiveProperty<bool> Bloom => _bloom;
        readonly ReactiveProperty<bool> _bloom = new(FileReadAndWriteUtility.UserProfile.IsBloom);

        public IReadOnlyReactiveProperty<bool> DepthOfField => _depthOfField;
        readonly ReactiveProperty<bool> _depthOfField = new(FileReadAndWriteUtility.UserProfile.IsDepthOfField);

        public IReadOnlyReactiveProperty<bool> Tonemapping => _tonemapping;
        readonly ReactiveProperty<bool> _tonemapping = new(FileReadAndWriteUtility.UserProfile.IsTonemapping);

        public IReadOnlyReactiveProperty<float> BloomThreshold => _bloomThreshold;
        readonly ReactiveProperty<float> _bloomThreshold = new(FileReadAndWriteUtility.UserProfile.BloomThreshold);

        public IReadOnlyReactiveProperty<float> BloomIntensity => _bloomIntensity;
        readonly ReactiveProperty<float> _bloomIntensity = new(FileReadAndWriteUtility.UserProfile.BloomIntensity);
        public IReadOnlyReactiveProperty<float> BloomColor => _bloomColor;
        readonly ReactiveProperty<float> _bloomColor = new(0.65f);//水色

        public IReadOnlyReactiveProperty<float> Outline => _outline;
        readonly ReactiveProperty<float> _outline = new(0);

        readonly RootAudioSourceService _audioSourceService;
        readonly GraphicsMenuSettings _settings;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GraphicsMenuService(
            RootAudioSourceService audioSourceService,
            GraphicsMenuSettings settings)
        {
            _audioSourceService = audioSourceService;
            _settings = settings;
        }

        public void Initialize()
        {
            // 購読前に初期化
            {
                _settings.GraphicsText[0].text = $"{_lightIntensity.Value:0.00}";
                _settings.GraphicsText[1].text = $"{_bloomThreshold.Value:0.00}";
                _settings.GraphicsText[2].text = $"{_bloomIntensity.Value:0.0}";
                _settings.GraphicsText[3].text = $"";//無し
                _settings.GraphicsText[4].text = _renderScale.Value.ToString();
                _settings.GraphicsText[5].text = _opaqueDownsampling.Value.AsString();
                _settings.GraphicsText[6].text = $"{0:0.00}";
                ChangeTextAntialiasingMode(_antialiasingMode.Value);
                ChangeTextMSAASamples(_msaaSamples.Value);
            }

            _settings.GraphicButton[1].isEnable = _bloom.Value;
            _settings.GraphicButton[2].isEnable = _depthOfField.Value;
            _settings.GraphicButton[3].isEnable = _tonemapping.Value;
            foreach (var button in _settings.GraphicButton)
            {
                button.OnTriggerAsObservable()
                    .Subscribe(OnClick).AddTo(_disposables);
            }
            _settings.AntialiasingButton[0].OnTriggerAsObservable()
                .Subscribe(_ => OnClickAntialiasingMode(0)).AddTo(_disposables);
            _settings.AntialiasingButton[1].OnTriggerAsObservable()
                .Subscribe(_ => OnClickAntialiasingMode(1)).AddTo(_disposables);
            _settings.MSAAButton[0].OnTriggerAsObservable()
                .Subscribe(_ => OnClickMSAASamples(0)).AddTo(_disposables);
            _settings.MSAAButton[1].OnTriggerAsObservable()
                .Subscribe(_ => OnClickMSAASamples(1)).AddTo(_disposables);

            _settings.GraphicSlider[0].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[0].text = $"{x:0.00}";
                    _lightIntensity.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[1].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[1].text = $"{x:0.00}";
                    _bloomThreshold.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[2].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[2].text = $"{x:0.0}";
                    _bloomIntensity.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[3].ValueAsObservable
                .Subscribe(x => _bloomColor.Value = x).AddTo(_disposables);
            _settings.GraphicSlider[4].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[4].text = x.ToString();
                    _renderScale.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[5].ValueAsObservable
                .Subscribe(x =>
                {
                    var samples = ((int)x).ToDownsamplingFromSlider();
                    _settings.GraphicsText[5].text = samples.AsString();
                    _opaqueDownsampling.Value = samples;
                }).AddTo(_disposables);
            _settings.GraphicSlider[6].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[6].text = $"{x:0.00}";
                    _outline.Value = x;
                }).AddTo(_disposables);

            _settings.GraphicSlider[0].Value = 1;
            _settings.GraphicSlider[1].Value = _bloomThreshold.Value;
            _settings.GraphicSlider[2].Value = _bloomIntensity.Value;
            _settings.GraphicSlider[3].Value = _bloomColor.Value;
            _settings.GraphicSlider[4].Value = _renderScale.Value;
            _settings.GraphicSlider[5].Value = _opaqueDownsampling.Value.ToDownsamplingSliderValue();
            _settings.GraphicSlider[6].Value = 0.3f;
        }

        void OnClick(Button_Base btn)
        {
            if (btn == _settings.GraphicButton[1])
            {
                _bloom.Value = btn.isEnable;
            }
            else if (btn == _settings.GraphicButton[2])
            {
                _depthOfField.Value = btn.isEnable;
            }
            else if (btn == _settings.GraphicButton[3])
            {
                _tonemapping.Value = btn.isEnable;
            }
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void OnClickAntialiasingMode(int btnIndex)
        {
            var moveIndex = btnIndex == 0 ? -1 : 1;
            var next = (int)_antialiasingMode.Value + moveIndex;
            var mode = (AntialiasingMode)Mathf.Clamp(next, (int)AntialiasingMode.None, (int)AntialiasingMode.SubpixelMorphologicalAntiAliasing);
            ChangeTextAntialiasingMode(mode);
            _antialiasingMode.Value = mode;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void OnClickMSAASamples(int btnIndex)
        {
            var moveIndex = btnIndex == 0 ? -1 : 1;
            var nowIndex = Array.IndexOf(_msaaOptions, _msaaSamples.Value);
            var nextIndex = Mathf.Clamp(nowIndex + moveIndex, 0, _msaaOptions.Length - 1);
            var samples = _msaaOptions[nextIndex];
            ChangeTextMSAASamples(samples);
            _msaaSamples.Value = samples;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void ChangeTextAntialiasingMode(AntialiasingMode antialiasingMode)
        {
            var isHeavyLoad = antialiasingMode == AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            _settings.AntialiasingText.text = antialiasingMode.AsString();
            _settings.AntialiasingText.color = isHeavyLoad ? Color.red : Color.white;
            
            if(antialiasingMode == AntialiasingMode.None)
            {
                _settings.AntialiasingInfoText[0].gameObject.SetActive(false);
                _settings.AntialiasingInfoText[1].gameObject.SetActive(false);
            }
            else if (antialiasingMode == AntialiasingMode.FastApproximateAntialiasing)
            {
                _settings.AntialiasingInfoText[0].gameObject.SetActive(true);
                _settings.AntialiasingInfoText[1].gameObject.SetActive(false);
            }
            else if (antialiasingMode == AntialiasingMode.SubpixelMorphologicalAntiAliasing)
            {
                _settings.AntialiasingInfoText[0].gameObject.SetActive(false);
                _settings.AntialiasingInfoText[1].gameObject.SetActive(true);
            }
        }

        void ChangeTextMSAASamples(MSAASamples msaaSamples)
        {
            var isAttentionLoad = msaaSamples == MSAASamples.MSAA4x;
            _settings.MSAAText.text = msaaSamples.AsString();
            _settings.MSAAText.color = isAttentionLoad ? Color.yellow : Color.white;
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}