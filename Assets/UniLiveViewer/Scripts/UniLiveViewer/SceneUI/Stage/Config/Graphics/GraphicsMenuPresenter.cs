using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Player;
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

            _graphicsMenuService.LightIntensity
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeLightIntensity)
                .AddTo(_disposables);
            _graphicsMenuService.AntialiasingMode
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeAntialiasing)
                .AddTo(_disposables);
            _graphicsMenuService.MASSSamples
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeMASS)
                .AddTo(_disposables);
            _graphicsMenuService.RenderScale
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeRenderScale)
                .AddTo(_disposables);
            _graphicsMenuService.Bloom
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeBloom)
                .AddTo(_disposables);
            _graphicsMenuService.DepthOfField
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeDepthOfField)
                .AddTo(_disposables);
            _graphicsMenuService.Tonemapping
                .SkipLatestValueOnSubscribe()
                .Subscribe(_graphicsSettingsService.ChangeTonemapping)
                .AddTo(_disposables);

            _graphicsMenuService.BloomThreshold
                .Subscribe(_graphicsSettingsService.ChangeBloomThreshold)
                .AddTo(_disposables);
            _graphicsMenuService.BloomIntensity
                .Subscribe(_graphicsSettingsService.ChangeBloomIntensity)
                .AddTo(_disposables);
            _graphicsMenuService.BloomColor
                .Subscribe(_graphicsSettingsService.ChangeBloomColor)
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
