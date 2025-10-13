using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Player.Graphics;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuPresenter : IStartable, IDisposable
    {
        readonly GraphicsMenuService _graphicsMenuService;
        readonly GraphicsSettingsService _graphicsSettingsService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GraphicsMenuPresenter(
            GraphicsMenuService graphicsMenuService,
            GraphicsSettingsService graphicsSettingsService)
        {
            _graphicsMenuService = graphicsMenuService;
            _graphicsSettingsService = graphicsSettingsService;
        }

        void IStartable.Start()
        {
            _graphicsMenuService.Initialize();

            _graphicsMenuService.AntialiasingModeValue
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeAntialiasing)
                .AddTo(_disposables);
            _graphicsMenuService.MSAASamplesValue
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeMSAA)
                .AddTo(_disposables);
            _graphicsMenuService.RenderScale
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeRenderScale)
                .AddTo(_disposables);
            _graphicsMenuService.OpaqueDownsampling
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeOpaqueDownsampling)
                .AddTo(_disposables);
            _graphicsMenuService.Bloom
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeBloom)
                .AddTo(_disposables);
            _graphicsMenuService.BloomResolutionScale
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeBloomResolutionScale)
                .AddTo(_disposables);
            _graphicsMenuService.BloomThreshold
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeBloomThreshold)
                .AddTo(_disposables);
            _graphicsMenuService.BloomIntensity
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeBloomIntensity)
                .AddTo(_disposables);
            _graphicsMenuService.BloomScatter
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeBloomScatter)
                .AddTo(_disposables);
            _graphicsMenuService.UseBloomColor
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeUseBloomColor)
                .AddTo(_disposables);
            _graphicsMenuService.BloomColorHue
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeBloomColorHue)
                .AddTo(_disposables);

            _graphicsMenuService.DepthOfField
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeDepthOfField)
                .AddTo(_disposables);
            _graphicsMenuService.Tonemapping
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeTonemapping)
                .AddTo(_disposables);

            _graphicsMenuService.Outline
                .Subscribe(_graphicsSettingsService.ChangeOutline)
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
