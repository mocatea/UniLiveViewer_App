using MessagePipe;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.Stage;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    [RequireComponent(typeof(AudioAssetManager), typeof(PlayableDirector))]
    [RequireComponent(typeof(QuasiShadowSetting))]
    public class TimelineLifetimeScope : LifetimeScope
    {
        [SerializeField] SpectrumConverter _spectrumConverter;
        [SerializeField] PresetResourceData _presetResourceData;
        [SerializeField] QuasiShadowSetting _quasiShadowSetting;
        [SerializeField] ActorLifetimeScopeSetting _actorLifetimeScopeSetting;

        /// <summary>
        /// Actorが使うのでここになちゃってる
        /// </summary>
        [SerializeField] GeneratorPortalAnchor _anchor;

        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<VRMLoadResultData>(options);
            builder.RegisterMessageBroker<AllActorOperationMessage>(options);
            builder.RegisterMessageBroker<ActorOperationMessage>(options);
            builder.RegisterMessageBroker<AllActorOptionMessage>(options);
            builder.RegisterMessageBroker<ActorAnimationMessage>(options);
            builder.RegisterMessageBroker<ActorStateMessage>(options);
            builder.RegisterMessageBroker<ActorResizeMessage>(options);
            builder.RegisterMessageBroker<AttachPointMessage>(options);

            builder.Register<VMDData>(Lifetime.Singleton);

            builder.RegisterInstance(_presetResourceData);
            builder.RegisterInstance(_quasiShadowSetting);

            builder.RegisterComponent(_anchor);
            builder.RegisterComponent(_spectrumConverter);
            builder.RegisterComponent(_actorLifetimeScopeSetting);
            
            builder.RegisterComponent(GetComponent<AudioAssetManager>());
            builder.RegisterComponent(GetComponent<PlayableDirector>());
            builder.Register<TimelineAudioClipSwitcherService>(Lifetime.Singleton);
            builder.Register<TimelineService>(Lifetime.Singleton);
            builder.Register<PlayableBinderService>(Lifetime.Singleton);
            builder.Register<PlayableAnimationClipService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayableMusicPresenter>();
            builder.RegisterEntryPoint<PlayableBinderPresenter>();
        }
    }
}
