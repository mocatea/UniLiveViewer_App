using System;
using UniLiveViewer.Player;
using UniRx;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;
using static UniLiveViewer.Player.GraphicsExtensions;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuService: IDisposable
    {
        const string Edge = "_Edge";

        public IReadOnlyReactiveProperty<float> LightIntensity => _lightIntensity;
        readonly ReactiveProperty<float> _lightIntensity = new(1);

        public IReadOnlyReactiveProperty<AntialiasingMode> AntialiasingMode => _antialiasingMode;
        readonly ReactiveProperty<AntialiasingMode> _antialiasingMode = new((AntialiasingMode)FileReadAndWriteUtility.UserProfile.Antialiasing);

        public IReadOnlyReactiveProperty<MSAASamples> MASSSamples => _msaaSamples;
        readonly ReactiveProperty<MSAASamples> _msaaSamples = new((MSAASamples)FileReadAndWriteUtility.UserProfile.MSAALevel);
        public IReadOnlyReactiveProperty<float> RenderScale => _renderScale;
        readonly ReactiveProperty<float> _renderScale = new(1);

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
                _settings.GraphicsText[3].text = $"{0:0.00}";
                _settings.GraphicsText[4].text = _antialiasingMode.Value.AsString();
                _settings.GraphicsText[5].text = _msaaSamples.Value.AsString();
                _settings.GraphicsText[6].text = _renderScale.Value.ToString();
            }

            _settings.GraphicButton[1].isEnable = _bloom.Value;
            _settings.GraphicButton[2].isEnable = _depthOfField.Value;
            _settings.GraphicButton[3].isEnable = _tonemapping.Value;
            foreach (var button in _settings.GraphicButton)
            {
                button.onTrigger += OnClick;
            }

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
                    var mode = (AntialiasingMode)x;
                    _settings.GraphicsText[4].text = mode.AsString();
                    _antialiasingMode.Value = mode;
                }).AddTo(_disposables);
            _settings.GraphicSlider[5].ValueAsObservable
                .Subscribe(x =>
                {
                    var samples = ((int)x).ToMSAALevelFromSlider();
                    _settings.GraphicsText[5].text = samples.AsString();
                    _msaaSamples.Value = samples;
                }).AddTo(_disposables);
            _settings.GraphicSlider[6].ValueAsObservable
                .Subscribe(x =>
                {
                    _settings.GraphicsText[6].text = x.ToString();
                    _renderScale.Value = x;
                }).AddTo(_disposables);
            _settings.OutlineSlider.ValueAsObservable
                .Subscribe(OnChangeOutline).AddTo(_disposables);

            _settings.GraphicSlider[0].Value = 1;
            _settings.GraphicSlider[1].Value = _bloomThreshold.Value;
            _settings.GraphicSlider[2].Value = _bloomIntensity.Value;
            _settings.GraphicSlider[3].Value = _bloomColor.Value;
            _settings.GraphicSlider[4].Value = (int)_antialiasingMode.Value;
            _settings.GraphicSlider[5].Value = _msaaSamples.Value.ToMSAASliderValue();
            _settings.GraphicSlider[6].Value = _renderScale.Value;
            _settings.OutlineSlider.Value = 0.3f;
            _settings.OutlineMat.SetFloat(Edge, _settings.OutlineSlider.Value);
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

        void OnChangeOutline(float value)
        {
            _settings.GraphicsText[3].text = $"{value:0.00}";

            if (_settings.OutlineRender == null) return;

            if (0 < value)
            {
                _settings.OutlineRender.SetActive(true);
                _settings.OutlineMat.SetFloat(Edge, value);
            }
            else _settings.OutlineRender.SetActive(false);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}