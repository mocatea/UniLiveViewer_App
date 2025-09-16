using System;
using UniLiveViewer.Stage;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuPresenter : IStartable, IDisposable
    {
        readonly StageLightingService _stageLightingService;
        readonly StageCommonMenuService _stageCommonMenuService;
        readonly IStageMenuService _stageMenuServie;
        readonly StageMenuOnEnableHandler _onEnableHandler;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public StageMenuPresenter(
            StageLightingService stageLightingService,
            StageCommonMenuService stageCommonMenuService,
            IStageMenuService stageMenuServie,
            StageMenuOnEnableHandler onEnableHandler)
        {
            _stageLightingService = stageLightingService;
            _stageCommonMenuService = stageCommonMenuService;
            _stageMenuServie = stageMenuServie;
            _onEnableHandler = onEnableHandler;
        }

        void IStartable.Start()
        {
            _stageCommonMenuService.Initialize(_stageLightingService.LightIntensity, _stageLightingService.LightRotationYow);
            _stageMenuServie.Initialize();
            _stageCommonMenuService.LightIntensity
                .SkipLatestValueOnSubscribe()
                .Subscribe(_stageLightingService.ChangeLightIntensity)
                .AddTo(_disposables);
            _stageCommonMenuService.LightRotation
                .SkipLatestValueOnSubscribe()
                .Subscribe(_stageLightingService.ChangeLightRotation)
                .AddTo(_disposables);
            _onEnableHandler.OnEnableAsObservable
                .Subscribe(x => _stageMenuServie.OnEnable())
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _stageCommonMenuService.Dispose();
            _stageMenuServie.Dispose();
            _disposables.Dispose();
        }
    }
}