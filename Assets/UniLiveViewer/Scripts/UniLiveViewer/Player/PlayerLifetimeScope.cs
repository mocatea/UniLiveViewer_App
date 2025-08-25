using MessagePipe;
using System.Collections.Generic;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.OVRCustom;
using UniLiveViewer.Player.HandMenu;
using UniLiveViewer.Timeline;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    [RequireComponent(typeof(PlayerRootAnchor))]
    [RequireComponent(typeof(PlayerRootAnchor))]
    [RequireComponent(typeof(CharacterCameraConstraint_Custom))]
    [RequireComponent(typeof(SimpleCapsuleWithStickMovement))]
    public class PlayerLifetimeScope : LifetimeScope
    {
        [SerializeField] VolumeProfile _volumeProfile;
        [SerializeField] PlayerConfigData _playerConfigData;

        [Header("XR設定")]
        [SerializeField] PlayerHandMenuAnchorL _playerHandMenuAnchorL;
        [SerializeField] PlayerHandMenuAnchorR _playerHandMenuAnchorR;
        [SerializeField] OVRManager _ovrManager;
        [SerializeField] PassthroughService _passthroughService;
        [SerializeField] PlayerHandMenuSettings _playerHandMenuSettings;
        /// <summary>
        /// inspector上のInject設定も必要(他手段もある)
        /// </summary>
        [SerializeField] List<OVRGrabber_UniLiveViewer> _ovrGrabbers = new();

        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<PlayerInputOperationMessage>(options);

            builder.RegisterInstance(_playerConfigData);
            builder.RegisterComponent<Camera>(Camera.main);

            builder.RegisterComponent(GetComponent<PlayerRootAnchor>());
            builder.Register<PlayerRootAnchorService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerRootAnchorPresenter>();

            builder.Register<PlayerInputService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerInputPresenter>();

            MetaConfigure(builder);
            GraphicsConfigure(builder);
            HandConfigure(builder);
            HandMenuConfigure(builder);
        }

        void MetaConfigure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_ovrManager);
            builder.RegisterComponent(GetComponent<CharacterCameraConstraint_Custom>());
            builder.RegisterComponent(GetComponent<SimpleCapsuleWithStickMovement>());
        }

        void GraphicsConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_volumeProfile);
            builder.RegisterComponent(_passthroughService);
            builder.Register<GraphicsSettingsService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GraphicsSettingsPresenter>();
        }

        void HandConfigure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_playerHandMenuAnchorL);
            builder.RegisterComponent(_playerHandMenuAnchorR);
            builder.RegisterComponent(_ovrGrabbers);
            builder.Register<PlayerHandsService>(Lifetime.Singleton);
            builder.Register<BothHandsHoldService>(Lifetime.Singleton);
        }

        void HandMenuConfigure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_playerHandMenuSettings);

            builder.Register<CameraHeightService>(Lifetime.Singleton);
            builder.Register<ActorManipulateService>(Lifetime.Singleton);
            builder.Register<ItemMaterialSelectionService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerHandMenuPresenter>();
        }
    }
}