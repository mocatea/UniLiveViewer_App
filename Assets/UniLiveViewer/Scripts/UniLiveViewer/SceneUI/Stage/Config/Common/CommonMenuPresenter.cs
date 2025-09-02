using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Common
{
    public class CommonMenuPresenter : IStartable, IDisposable
    {
        readonly CommonMenuSettings _settings;
        readonly CommonMenuService _commonMenuService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public CommonMenuPresenter(
            CommonMenuSettings settings,
            CommonMenuService commonMenuService)
        {
            _settings = settings;
            _commonMenuService = commonMenuService;
        }

        void IStartable.Start()
        {
            _settings.FixedFoveatedSlider.ValueAsObservable
                .Subscribe(_commonMenuService.OnUpdateFixedFoveated).AddTo(_disposables);

            _commonMenuService.Initialize();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
            _commonMenuService.Dispose();
        }
    }
}