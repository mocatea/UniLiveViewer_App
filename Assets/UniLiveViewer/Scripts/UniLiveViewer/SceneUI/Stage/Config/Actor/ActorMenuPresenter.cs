using MessagePipe;
using System;
using UniLiveViewer.Actor;
using UniLiveViewer.Actor.Option;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Actor
{
    public class ActorMenuPresenter : IStartable, IDisposable
    {
        readonly IPublisher<AllActorOperationMessage> _allPublisher;
        readonly ActorMenuService _actorMenuService;
        readonly ActorMenuSettings _settings;
        readonly QuasiShadowSetting _quasiShadowSetting;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public ActorMenuPresenter(
            IPublisher<AllActorOperationMessage> allPublisher,
            ActorMenuService actorMenuService,
            ActorMenuSettings settings,
            QuasiShadowSetting quasiShadowSetting)
        {
            _allPublisher = allPublisher;
            _actorMenuService = actorMenuService;
            _settings = settings;
            _quasiShadowSetting = quasiShadowSetting;
        }

        void IStartable.Start()
        {
            // アクターサイズ
            _settings.InitialActorSizeSlider.ValueAsObservable
                .Subscribe(OnActorSizeValueChanged).AddTo(_disposables);
            _settings.InitialActorSizeSlider.EndDriveAsObservable
                .Subscribe(_ => OnActorSizeChangeCommitted()).AddTo(_disposables);

            // 落ち影種類
            _settings.FallingShadowLButton.OnTriggerAsObservable()
                .Subscribe(_ =>
                {
                    _quasiShadowSetting.ShadowType -= 1;
                    OnChangeFallingShadowType(_quasiShadowSetting.ShadowType);
                }).AddTo(_disposables);
            _settings.FallingShadowRButton.OnTriggerAsObservable()
                .Subscribe(_ =>
                {
                    _quasiShadowSetting.ShadowType += 1;
                    OnChangeFallingShadowType(_quasiShadowSetting.ShadowType);
                }).AddTo(_disposables);
            // 落ち影サイズ
            _settings.FallingShadowSlider.ValueAsObservable
                .Subscribe(OnFallingShadowValueChanged).AddTo(_disposables);
            _settings.FallingShadowSlider.EndDriveAsObservable
                .Subscribe(_ => OnFallingShadowSizeChangeCommitted()).AddTo(_disposables);

            _settings.InitialActorSizeSlider.SetValueWithoutNotify(FileReadAndWriteUtility.UserProfile.InitCharaSize);
            _settings.InitialActorSizeText.text = $"{FileReadAndWriteUtility.UserProfile.InitCharaSize:0.00}";

            var shadowType = (SHADOWTYPE)FileReadAndWriteUtility.UserProfile.CharaShadowType;
            _quasiShadowSetting.ShadowType = shadowType;
            _settings.FallingShadowTypeText.text = $"{shadowType}";

            _settings.FallingShadowSlider.SetValueWithoutNotify(FileReadAndWriteUtility.UserProfile.CharaShadowSize);
            _quasiShadowSetting.SetShadowScale(FileReadAndWriteUtility.UserProfile.CharaShadowSize);
            _settings.FallingShadowValueText.text = $"{FileReadAndWriteUtility.UserProfile.CharaShadowSize:0.00}";
        }

        void OnActorSizeValueChanged(float value)
        {
            _settings.InitialActorSizeText.text = $"{value:0.00}";
        }

        void OnActorSizeChangeCommitted()
        {
            _actorMenuService.ApplyActorSizeValue(_settings.InitialActorSizeSlider.Value);
        }

        void OnFallingShadowValueChanged(float value)
        {
            _quasiShadowSetting.SetShadowScale(value);
            _settings.FallingShadowValueText.text = $"{value:0.00}";

            // リアルタイムに反映
            var message = new AllActorOperationMessage(ActorState.FIELD, ActorCommand.UPDATE_SHADOW);
            _allPublisher.Publish(message);
        }

        void OnFallingShadowSizeChangeCommitted()
        {
            _actorMenuService.ApplyFallingShadowSizeValue(_settings.FallingShadowSlider.Value);
        }

        void OnChangeFallingShadowType(SHADOWTYPE type)
        {
            _settings.FallingShadowTypeText.text = $"{type}";
            _actorMenuService.ApplyFallingShadowTypeValue(type);

            //json保存後に通知
            var message = new AllActorOperationMessage(ActorState.FIELD, ActorCommand.UPDATE_SHADOW);
            _allPublisher.Publish(message);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}