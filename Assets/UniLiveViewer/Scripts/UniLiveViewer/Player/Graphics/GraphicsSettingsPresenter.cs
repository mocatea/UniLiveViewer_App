using System;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player.Graphics
{
    public class GraphicsSettingsPresenter : IStartable, IDisposable
    {
        readonly PassthroughService _passthroughService;
        readonly GraphicsSettingsService _graphicsSettingsService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GraphicsSettingsPresenter(
            GraphicsSettingsService graphicsSettingsService,
            PassthroughService passthroughService)
        {
            _graphicsSettingsService = graphicsSettingsService;
            _passthroughService = passthroughService;
        }

        void IStartable.Start()
        {
            _graphicsSettingsService.Initialize();
            _passthroughService.Initialize();

            _passthroughService.IsEnable
                .Subscribe(_graphicsSettingsService.OnChangePassthrough)
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
