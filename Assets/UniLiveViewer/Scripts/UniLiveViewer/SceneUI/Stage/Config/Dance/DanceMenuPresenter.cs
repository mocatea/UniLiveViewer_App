using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Dance
{
    public class DanceMenuPresenter : IStartable, IDisposable
    {
        readonly DanceMenuSettings _settings;
        readonly DanceMenuService _danceMenuService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public DanceMenuPresenter(
            DanceMenuSettings danceMenuSettings,
            DanceMenuService danceMenuService)
        {
            _settings = danceMenuSettings;
            _danceMenuService = danceMenuService;
        }

        void IStartable.Start()
        {
            _settings.VMDScaleSlider.EndDriveAsObservable
                .Subscribe(_ => _danceMenuService.OnUnControledVMDScale()).AddTo(_disposables);
            _settings.VMDScaleSlider.ValueAsObservable
                .Subscribe(_danceMenuService.OnUpdateVMDScale).AddTo(_disposables);
            _danceMenuService.Initialize();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
            _danceMenuService.Dispose();
        }
    }
}