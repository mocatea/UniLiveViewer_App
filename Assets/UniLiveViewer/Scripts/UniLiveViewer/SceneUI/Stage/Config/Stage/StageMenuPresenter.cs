using System;
using VContainer;
using VContainer.Unity;
using UniRx;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuPresenter : IStartable , IDisposable
    {
        readonly IStageMenuService _stageMenuServie;
        readonly StageMenuOnEnableHandler _onEnableHandler;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public StageMenuPresenter(
            IStageMenuService stageMenuServie,
            StageMenuOnEnableHandler onEnableHandler)
        {
            _stageMenuServie = stageMenuServie;
            _onEnableHandler = onEnableHandler;
        }

        void IStartable.Start()
        {
            _stageMenuServie.Initialize();
            _onEnableHandler.OnEnableAsObservable
                .Subscribe(x => _stageMenuServie.OnEnable())
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
            _stageMenuServie.Dispose();
        }
    }
}