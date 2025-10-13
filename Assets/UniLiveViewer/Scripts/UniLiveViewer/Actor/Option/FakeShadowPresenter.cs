using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
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
            var presetIndex = FileReadAndWriteUtility.UserProfile.CharaShadowType;
            var shadowType = (SHADOWTYPE)presetIndex;
            var userShadowScale = FileReadAndWriteUtility.UserProfile.CharaShadowSize;
            _fakeShadowService.Setup(shadowType, userShadowScale, _setting, presetIndex);

            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorState != ActorState.FIELD) return;
                    if (x.ActorCommand != ActorCommand.UPDATE_SHADOW) return;
                    var presetIndex = (int)_setting.ShadowType;
                    _fakeShadowService.OnUpdateShadowSettings(_setting.ShadowType, _setting.ShadowScale, _setting, presetIndex);
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
