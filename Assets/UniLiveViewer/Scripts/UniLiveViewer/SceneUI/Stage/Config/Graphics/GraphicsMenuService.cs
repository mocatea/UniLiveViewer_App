using System;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;
using static UniLiveViewer.Player.Graphics.GraphicsExtensions;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuService : IDisposable
    {
        public IReadOnlyReactiveProperty<AntialiasingMode> AntialiasingModeValue => _antialiasingMode;
        readonly ReactiveProperty<AntialiasingMode> _antialiasingMode = new((AntialiasingMode)FileReadAndWriteUtility.UserProfile.Antialiasing);

        public IReadOnlyReactiveProperty<MSAASamples> MSAASamplesValue => _msaaSamples;
        readonly ReactiveProperty<MSAASamples> _msaaSamples = new((MSAASamples)FileReadAndWriteUtility.UserProfile.MSAALevel);
        readonly MSAASamples[] _msaaOptions = new MSAASamples[] { MSAASamples.None, MSAASamples.MSAA2x/*, MSAASamples.MSAA4x*/ };// なんで数値飛んでるのメンド

        public IReadOnlyReactiveProperty<float> RenderScale => _renderScale;
        readonly ReactiveProperty<float> _renderScale = new(1);

        public IReadOnlyReactiveProperty<Downsampling> OpaqueDownsampling => _opaqueDownsampling;
        readonly ReactiveProperty<Downsampling> _opaqueDownsampling = new(Downsampling._2xBilinear);

        public IReadOnlyReactiveProperty<bool> Bloom => _bloom;
        readonly ReactiveProperty<bool> _bloom = new(FileReadAndWriteUtility.UserProfile.IsBloom);

        public IReadOnlyReactiveProperty<float> BloomResolutionScale => _bloomResolutionScale;
        readonly ReactiveProperty<float> _bloomResolutionScale = new(1);

        public IReadOnlyReactiveProperty<float> BloomThreshold => _bloomThreshold;
        readonly ReactiveProperty<float> _bloomThreshold = new(FileReadAndWriteUtility.UserProfile.BloomThreshold);

        public IReadOnlyReactiveProperty<float> BloomIntensity => _bloomIntensity;
        readonly ReactiveProperty<float> _bloomIntensity = new(FileReadAndWriteUtility.UserProfile.BloomIntensity);

        public IReadOnlyReactiveProperty<float> BloomScatter => _bloomScatter;
        readonly ReactiveProperty<float> _bloomScatter = new(0.5f);

        public IReadOnlyReactiveProperty<bool> UseBloomColor => _useBloomColor;
        readonly ReactiveProperty<bool> _useBloomColor = new();

        public IReadOnlyReactiveProperty<float> BloomColor => _bloomColor;
        readonly ReactiveProperty<float> _bloomColor = new(0.65f);//水色

        public IReadOnlyReactiveProperty<bool> DepthOfField => _depthOfField;
        readonly ReactiveProperty<bool> _depthOfField = new(FileReadAndWriteUtility.UserProfile.IsDepthOfField);

        public IReadOnlyReactiveProperty<bool> Tonemapping => _tonemapping;
        readonly ReactiveProperty<bool> _tonemapping = new(FileReadAndWriteUtility.UserProfile.IsTonemapping);

        public IReadOnlyReactiveProperty<float> Outline => _outline;
        readonly ReactiveProperty<float> _outline = new(0.3f);

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
                _settings.GraphicsText[0].text = $"{_bloomThreshold.Value:0.00}";
                _settings.GraphicsText[1].text = $"{_bloomIntensity.Value:0.0}";
                _settings.GraphicsText[2].text = $"";//無し
                _settings.GraphicsText[3].text = _renderScale.Value.ToString();
                _settings.GraphicsText[4].text = _opaqueDownsampling.Value.AsString();
                _settings.GraphicsText[5].text = $"{_outline.Value:0.00}";
                _settings.GraphicsText[6].text = $"{_bloomResolutionScale.Value:0.00}";
                _settings.GraphicsText[7].text = $"{_bloomScatter.Value:0.00}";
                ChangeTextAntialiasingMode(_antialiasingMode.Value);
                ChangeTextMSAASamples(_msaaSamples.Value);
            }

            _settings.GraphicButton[1].isEnable = _bloom.Value;
            _settings.GraphicButton[2].isEnable = _depthOfField.Value;
            _settings.GraphicButton[3].isEnable = _tonemapping.Value;
            _settings.GraphicButton[4].isEnable = _useBloomColor.Value;
            _settings.BloomClolorGroup.gameObject.SetActive(_useBloomColor.Value);
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
                    _bloomThreshold.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[1].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[1].text = $"{x:0.0}";
                    _bloomIntensity.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[2].ValueAsObservable
                .Subscribe(x => _bloomColor.Value = x).AddTo(_disposables);
            _settings.GraphicSlider[3].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[3].text = x.ToString();
                    _renderScale.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[4].ValueAsObservable
                .Subscribe(x =>
                {
                    var samples = ((int)x).ToDownsamplingFromSlider();
                    _settings.GraphicsText[4].text = samples.AsString();
                    _opaqueDownsampling.Value = samples;
                }).AddTo(_disposables);
            _settings.GraphicSlider[5].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[5].text = $"{x:0.00}";
                    _outline.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[6].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[6].text = $"{x:0.00}";
                    _bloomResolutionScale.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[7].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[7].text = $"{x:0.0}";
                    _bloomScatter.Value = x;
                }).AddTo(_disposables);

            _settings.GraphicSlider[0].Value = _bloomThreshold.Value;
            _settings.GraphicSlider[1].Value = _bloomIntensity.Value;
            _settings.GraphicSlider[2].Value = _bloomColor.Value;
            _settings.GraphicSlider[3].Value = _renderScale.Value;
            _settings.GraphicSlider[4].Value = _opaqueDownsampling.Value.ToDownsamplingSliderValue();
            _settings.GraphicSlider[5].Value = _outline.Value;
            _settings.GraphicSlider[6].Value = _bloomResolutionScale.Value;
            _settings.GraphicSlider[7].Value = _bloomScatter.Value;
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
            else if (btn == _settings.GraphicButton[4])
            {
                _useBloomColor.Value = btn.isEnable;
                _settings.BloomClolorGroup.gameObject.SetActive(btn.isEnable);
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
            _settings.AntialiasingText.color = isHeavyLoad ? Color.yellow : Color.white;

            if (antialiasingMode == AntialiasingMode.None)
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
            _settings.MSAAText.text = msaaSamples.AsString();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}