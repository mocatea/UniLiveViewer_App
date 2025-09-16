using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// Actorページ用
    /// 
    /// NOTE: ロジック多くて既に悪い..
    /// </summary>
    public class ActorPresenter : IAsyncStartable, IDisposable
    {
        CurrentMode _actorCurrentMode = CurrentMode.PRESET;
        CurrentMode _animationCurrentMode = CurrentMode.PRESET;
        bool _isReverse;
        int _clipIndex = 0;


        readonly ISubscriber<VRMLoadResultData> _vrmLoadSubscriber;
        readonly IPublisher<VRMMenuShowMessage> _publisher;
        readonly IPublisher<ActorAnimationMessage> _animationPublisher;
        readonly VMDData _vmdData;
        readonly CharacterPage _characterPage;
        readonly ActorEntityManagerService _actorEntityManager;
        readonly JumpList _jumpList;
        readonly PlayableBinderService _playableBinderService;

        readonly CompositeDisposable _disposables = new();
        /// <summary>
        /// 常に1つだけ購読用
        /// </summary>
        readonly SerialDisposable _serialDisposable = new();

        [Inject]
        public ActorPresenter(
            ISubscriber<VRMLoadResultData> vrmLoadSubscriber,
            IPublisher<VRMMenuShowMessage> publisher,
            IPublisher<ActorAnimationMessage> animationPublisher,
            VMDData vmdData,
            CharacterPage characterPage,
            ActorEntityManagerService actorEntityManager,
            JumpList jumpList,
            PlayableBinderService playableBinderService)
        {
            _vrmLoadSubscriber = vrmLoadSubscriber;
            _publisher = publisher;
            _animationPublisher = animationPublisher;
            _vmdData = vmdData;
            _characterPage = characterPage;
            _actorEntityManager = actorEntityManager;
            _jumpList = jumpList;
            _playableBinderService = playableBinderService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _vrmLoadSubscriber
                .Subscribe(vrmResultData =>
                {
                    _characterPage.OnLoadedVRM(vrmResultData);
                }).AddTo(_disposables);

            // 購読より先に
            _actorEntityManager.RegisterFBX();
            //VRMロード枠用にnull登録
            _actorEntityManager.FastRegisterVRM();

            _playableBinderService.StageActorCount
                .Subscribe(OnFieldCharacterCount).AddTo(_disposables);
            _playableBinderService.BindingToAsObservable
                .Subscribe(_ => OnBindingTo()).AddTo(_disposables);

            _characterPage.FBXIndex
                .SkipLatestValueOnSubscribe()
                .Subscribe(x =>
                {
                    ActiveFBXAsync(x, cancellation).Forget();
                }).AddTo(_disposables);
            _characterPage.VRMIndex
                .SkipLatestValueOnSubscribe()
                .Subscribe(x =>
                {
                    ActiveVRMAsync(x, cancellation).Forget();
                }).AddTo(_disposables);

            _characterPage.IsReverse
                .SkipLatestValueOnSubscribe()
                .Subscribe(ReverseClip).AddTo(_disposables);
            _characterPage.ClipIndex
                .SkipLatestValueOnSubscribe()
                .Subscribe(ActiveClip).AddTo(_disposables);
            _characterPage.VMDIndex
                .SkipLatestValueOnSubscribe()
                .Subscribe(ActiveVMD).AddTo(_disposables);

            _jumpList.OnSelectAsObservable
                .Subscribe(_characterPage.OnJumpSelect).AddTo(_disposables);

            _characterPage.OnStart();

            await UniTask.CompletedTask;
        }

        void OnFieldCharacterCount(int i)
        {
            _characterPage.OnUpdateActorCount();
        }

        void OnBindingTo()
        {
            if (_actorCurrentMode == CurrentMode.PRESET)
            {
                _actorEntityManager.RemoveFBX(_characterPage.FBXIndex.Value);
            }
            else if (_actorCurrentMode == CurrentMode.CUSTOM)
            {
                _actorEntityManager.RemoveVRM(_characterPage.VRMIndex.Value);
            }
        }

        async UniTask ActiveFBXAsync(int index, CancellationToken cancellation)
        {
            if (SystemInfo.MaxFieldActor <= _playableBinderService.StageActorCount.Value) return;

            _actorCurrentMode = CurrentMode.PRESET;
            _publisher.Publish(new VRMMenuShowMessage(-1));

            var actor = await _actorEntityManager.ActiveFBXAsync(index, cancellation);
            if (actor == null) return;

            // AddToは挙動異なるのでNG
            _serialDisposable.Disposable = actor.ActorEntity()
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    if (!_actorEntityManager.TryGetCurrentInstaceID(out var instanceId)) return;
                    _playableBinderService.BindingNewActor(instanceId, actor);
                    _characterPage.OnBindingNewActor(x);
                });
        }

        async UniTask ActiveVRMAsync(int index, CancellationToken cancellation)
        {
            if (SystemInfo.MaxFieldActor <= _playableBinderService.StageActorCount.Value) return;

            _actorCurrentMode = CurrentMode.CUSTOM;

            if (index == 0)
            {
                _publisher.Publish(new VRMMenuShowMessage(0));
                _actorEntityManager.SendAllActorDisableMessage();
                _playableBinderService.PortalUnbind();
                _characterPage.OnVRMLoadFrame();
            }
            else
            {
                _publisher.Publish(new VRMMenuShowMessage(-1));
                var actor = await _actorEntityManager.ActiveVRMAsync(index, cancellation);
                if (actor == null) return;

                // AddToは挙動異なるのでNG
                _serialDisposable.Disposable = actor.ActorEntity()
                    .Where(x => x != null)
                    .Subscribe(x =>
                    {
                        if (!_actorEntityManager.TryGetCurrentInstaceID(out var instanceId)) return;
                        _playableBinderService.BindingNewActor(instanceId, actor);
                        _characterPage.OnBindingNewActor(x);
                    });
            }
        }

        void ActiveClip(int index)
        {
            _animationCurrentMode = CurrentMode.PRESET;
            if (!_actorEntityManager.TryGetCurrentInstaceID(out var instanceId)) return;
            _clipIndex = index;
            var message = new ActorAnimationMessage(instanceId, _animationCurrentMode, _isReverse, _clipIndex);
            _animationPublisher.Publish(message);
            _characterPage.OnBindingNewAnimation();
        }

        void ReverseClip(bool isReverse)
        {
            _isReverse = isReverse;
            if (!_actorEntityManager.TryGetCurrentInstaceID(out var instanceId)) return;
            var message = new ActorAnimationMessage(instanceId, _animationCurrentMode, _isReverse, _clipIndex);
            _animationPublisher.Publish(message);
            _characterPage.OnBindingNewAnimation();
        }

        void ActiveVMD(int index)
        {
            _animationCurrentMode = CurrentMode.CUSTOM;
            _vmdData.UpdateCurrent(index);
            if (!_actorEntityManager.TryGetCurrentInstaceID(out var instanceId)) return;
            var message = new ActorAnimationMessage(instanceId, _animationCurrentMode, false, index);
            _animationPublisher.Publish(message);
            _characterPage.OnBindingNewAnimation();
        }

        void IDisposable.Dispose()
        {
            _characterPage.Dispose();
            _disposables.Dispose();
            _serialDisposable.Dispose();
        }
    }
}
