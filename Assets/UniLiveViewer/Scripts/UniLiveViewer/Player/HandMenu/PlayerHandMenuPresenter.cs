using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player.HandMenu
{
    public class PlayerHandMenuPresenter : IStartable, ILateTickable, IDisposable
    {
        readonly PlayableDirector _playableDirector;
        readonly IPublisher<AttachPointMessage> _publisher;
        readonly PlayerInputService _playerInputService;
        readonly CameraHeightService _cameraHeightService;
        readonly ActorManipulateService _actorManipulateService;
        readonly ItemMaterialSelectionService _itemMaterialSelection;
        readonly BothHandsHoldService _bothHandsHoldService;
        readonly PlayerHandsService _playerHandsService;

        readonly CompositeDisposable _disposables = new();
        /// <summary>
        /// 常に1つだけ購読用
        /// </summary>
        readonly SerialDisposable _serialDisposable = new();

        [Inject]
        public PlayerHandMenuPresenter(
            PlayableDirector playableDirector,
            IPublisher<AttachPointMessage> publisher,
            PlayerInputService playerInputService,
            CameraHeightService cameraHeightService,
            ActorManipulateService actorManipulateService,
            ItemMaterialSelectionService itemMaterialSelection,
            BothHandsHoldService bothHandsHoldService,
            PlayerHandsService playerHandsService)
        {
            _playableDirector = playableDirector;
            _publisher = publisher;
            _playerInputService = playerInputService;
            _cameraHeightService = cameraHeightService;
            _actorManipulateService = actorManipulateService;
            _itemMaterialSelection = itemMaterialSelection;
            _bothHandsHoldService = bothHandsHoldService;
            _playerHandsService = playerHandsService;
        }

        void IStartable.Start()
        {
            _playerInputService.ClickStickUpAsObservable()
                .Where(x => x == PlayerHandType.LHand)
                .Subscribe(_ => _cameraHeightService.OnClickStickUp())
                .AddTo(_disposables);
            _playerInputService.ClickStickDownAsObservable()
                .Where(x => x == PlayerHandType.LHand)
                .Subscribe(_ => _cameraHeightService.OnClickStickDown())
                .AddTo(_disposables);

            _playerInputService.ClickMenuAsObservable()
                .Where(x => x == PlayerHandType.LHand)
                .Where(_ => _playerHandsService.IsHandsFree())//両手checkは過剰かも
                .Subscribe(_ => _cameraHeightService.ChangeShow())
                .AddTo(_disposables);
            _playerInputService.ClickActionAsObservable()
                .Subscribe(_playerHandsService.OnClickActionButton)
                .AddTo(_disposables);
            _playerInputService.ClickTriggerAsObservable()
                .Subscribe(_playerHandsService.OnClickTriggerButton)
                .AddTo(_disposables);
            _playerInputService.ClickStickLeftAsObservable()
                .Subscribe(_playerHandsService.OnClickStickLeft)
                .AddTo(_disposables);
            _playerInputService.ClickStickRightAsObservable()
                .Subscribe(_playerHandsService.OnClickStickRight)
                .AddTo(_disposables);

            _playerInputService.LeftStickInput()
                .SkipLatestValueOnSubscribe()
                .Subscribe(x => _playerHandsService.OnChangeStickInput(PlayerHandType.LHand, x))
                .AddTo(_disposables);
            _playerInputService.RightStickInput()
                .SkipLatestValueOnSubscribe()
                .Subscribe(x => _playerHandsService.OnChangeStickInput(PlayerHandType.RHand, x))
                .AddTo(_disposables);

            PlayerHandSubscribe(PlayerHandType.LHand);
            PlayerHandSubscribe(PlayerHandType.RHand);

            _cameraHeightService.Setup();
            _actorManipulateService.Setup();
            _itemMaterialSelection.Setup();
            _bothHandsHoldService.Setup();
        }

        // NOTE: 循環してるけど目をつぶる（両手をLS化しないとキツイ
        void PlayerHandSubscribe(PlayerHandType targetHandType)
        {
            _playerHandsService.GrabbedObj(targetHandType)
                    .Subscribe(OnChangeGrabbedObj).AddTo(_disposables);

            _playerHandsService.HandActionStateAsObservable(targetHandType)
                .Where(x => x.Target == HandTargetType.Actor)
                .Subscribe(x =>
                {
                    if (x.Action == HandActionState.Grab)
                    {
                        _actorManipulateService.ChangeShow(targetHandType, true);
                    }
                    else if (x.Action == HandActionState.Release)
                    {
                        _actorManipulateService.ChangeShow(targetHandType, false);
                        _playerHandsService.IfNeededDeleteGuide();
                    }
                }).AddTo(_disposables);

            _playerHandsService.HandActionStateAsObservable(targetHandType)
                .Where(x => x.Target == HandTargetType.Item)
                .Subscribe(x =>
                {
                    var ovrGrabber = _playerHandsService.GetOVRGrabber(targetHandType);
                    if (x.Action == HandActionState.Grab)
                    {
                        _bothHandsHoldService.BothHandsCandidate(ovrGrabber);
                        if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
                        {
                            _publisher.Publish(new AttachPointMessage(true));
                        }
                    }
                    else if (x.Action == HandActionState.Release)
                    {
                        _bothHandsHoldService.BothHandsGrabEnd(ovrGrabber);
                        if (_playerHandsService.IsHandsFree())
                        {
                            _publisher.Publish(new AttachPointMessage(false));
                        }
                    }
                }).AddTo(_disposables);
        }

        /// <summary>
        /// 握っている間のみ購読
        /// </summary>
        void OnChangeGrabbedObj(OVRGrabbable ovrGrabbable)
        {
            if (ovrGrabbable == null)
            {
                _serialDisposable.Disposable = null;
            }
            else if (ovrGrabbable.TryGetComponent<Actor.ActorLifetimeScope>(out var actor))
            {
                var actorEntity = actor.Container.Resolve<Actor.IActorEntity>();
                // TODO: ActorResizeMessageを直で購読してないのは
                // scale変更が差分だからactor経由必須なのか...課題
                _serialDisposable.Disposable = actorEntity.RootScalar()
                .Subscribe(x =>
                {
                    _actorManipulateService.OnChangeActorSize(x);
                });
            }
            else
            {
                _serialDisposable.Disposable = null;
            }
        }

        void ILateTickable.LateTick()
        {
            _cameraHeightService.OnLateTick();
            _actorManipulateService.OnLateTick();
            _itemMaterialSelection.OnLateTick();
            _bothHandsHoldService.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}