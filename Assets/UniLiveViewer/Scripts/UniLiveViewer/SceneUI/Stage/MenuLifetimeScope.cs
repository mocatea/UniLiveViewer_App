using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    [RequireComponent(typeof(MenuManager))]
    public class MenuLifetimeScope : LifetimeScope
    {
        [SerializeField] BookSetting _bookSetting;
        [SerializeField] BookAnchor _bookAnchor;

        // この辺全部LS化したい
        [Header("各ページ")]
        [SerializeField] CharacterPage _characterPage;
        [SerializeField] AudioPlaybackPage _audioPlaybackPage;
        [SerializeField] ItemPage _itemPage;

        [Header("その他")]
        [SerializeField] JumpList _jumpList;

        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<VRMMenuShowMessage>(options);

            ActorPageConfigure(builder);

            builder.RegisterComponent(GetComponent<MenuManager>());

            builder.RegisterComponent(_audioPlaybackPage);
            builder.RegisterComponent(_itemPage);
            builder.RegisterEntryPoint<MainMenuPresenter>();

            builder.RegisterComponent(_bookSetting);
            builder.RegisterComponent(_bookAnchor);
            builder.Register<BookService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BookPresenter>();
        }

        /// <summary>
        /// 理想はページごとにLS分けたい
        /// </summary>
        /// <param name="builder"></param>
        void ActorPageConfigure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_characterPage);
            builder.RegisterComponent(_jumpList);
            builder.Register<ActorEntityFactory>(Lifetime.Singleton);
            builder.Register<ActorRegisterService>(Lifetime.Singleton);
            builder.Register<ActorEntityManagerService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ActorPresenter>();
        }
    }
}
