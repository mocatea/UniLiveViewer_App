using MessagePipe;
using UniLiveViewer.Menu;
using UniLiveViewer.MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    [RequireComponent(typeof(PlayerHandVRMCollidersService))]
    public class StageLifetimeScope : LifetimeScope
    {
        [SerializeField] BlackoutCurtain _blackoutCurtain;
        [SerializeField] PlayerHandVRMCollidersService _playerHandVRMCollidersService;

        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<PassthroughMessage>(options);

            builder.RegisterComponent(_blackoutCurtain);
            builder.RegisterComponent(_playerHandVRMCollidersService);
            builder.RegisterEntryPoint<StagePresenter>();
        }
    }
}
