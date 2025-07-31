using MessagePipe;
using System;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    public class FakeShadowPresenter : IStartable, ITickable, IDisposable
    {
        readonly IActorEntity _actorEntity;
        readonly FakeShadowService _fakeShadowService;
        readonly ISubscriber<AllActorOperationMessage> _allSubscriber;
        readonly QuasiShadowSetting _setting;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FakeShadowPresenter(
            IActorEntity actorService,
            FakeShadowService fakeShadowService,
            QuasiShadowSetting setting,
            ISubscriber<AllActorOperationMessage> allSubscriber)
        {
            _actorEntity = actorService;
            _fakeShadowService = fakeShadowService;
            _setting = setting;
            _allSubscriber = allSubscriber;
        }

        void IStartable.Start()
        {
            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorState != ActorState.FIELD) return;
                    if (x.ActorCommand != ActorCommand.UPDATE_SHADOW) return;
                    var presetIndex = (int)_setting.ShadowType;
                    _fakeShadowService.OnUpdateShadowSettings(_setting.ShadowType, _setting.ShadowScale, _setting.Presets[presetIndex]);
                }).AddTo(_disposables);

            _actorEntity.ActorEntity()
                .Subscribe(_fakeShadowService.OnChangeActorEntity)
                .AddTo(_disposables);
            _actorEntity.ActorState()
                .Select(x => x == ActorState.FIELD)
                .Subscribe(_fakeShadowService.SetEnable)
                .AddTo(_disposables);
            _actorEntity.RootScalar()
                .Subscribe(_fakeShadowService.OnChangeRootScalar)
                .AddTo(_disposables);

            var presetIndex = FileReadAndWriteUtility.UserProfile.CharaShadowType;
            var shadowType = (SHADOWTYPE)presetIndex;
            var shadowScale = FileReadAndWriteUtility.UserProfile.CharaShadowSize;
            _fakeShadowService.Setup(shadowType, shadowScale, _setting.Presets[presetIndex]);
        }

        void ITickable.Tick()
        {
            if (!_actorEntity.Active().Value) return;
            _fakeShadowService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _fakeShadowService.Dispose();
            _disposables.Dispose();
        }
    }
}
