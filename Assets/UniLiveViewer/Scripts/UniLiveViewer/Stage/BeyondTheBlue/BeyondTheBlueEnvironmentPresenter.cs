using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Menu.Config.Stage;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class BeyondTheBlueEnvironmentPresenter : IStartable, IDisposable
    {
        readonly IStageMenuService _stageMenuServie;
        readonly BeyondTheBlueEnvironmentService _environmentService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public BeyondTheBlueEnvironmentPresenter(
            IStageMenuService stageMenuServie,
            BeyondTheBlueEnvironmentService environmentService)
        {
            _stageMenuServie = stageMenuServie;
            _environmentService = environmentService;
        }

        void IStartable.Start()
        {
            if (_stageMenuServie is BeyondTheBlueMenuServie menuServie)
            {
                menuServie.PropSet
                    .SkipLatestValueOnSubscribe()
                    .Subscribe(_environmentService.OnClickPropSet)
                    .AddTo(_disposable);
                menuServie.IsGodRayAsObservable
                    .Subscribe(_environmentService.OnClickGodRay)
                    .AddTo(_disposable);
                menuServie.WaterColor
                    .SkipLatestValueOnSubscribe()
                    .Subscribe(_environmentService.OnChangeWaterColor)
                    .AddTo(_disposable);
            }
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}