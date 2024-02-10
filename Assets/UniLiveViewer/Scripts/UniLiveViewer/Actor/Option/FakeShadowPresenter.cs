﻿using System;
using UniLiveViewer.Actor;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    public class FakeShadowPresenter : IStartable, ITickable, IDisposable
    {
        bool _isTick;

        readonly IActorEntity _actorEntity;
        readonly FakeShadowService _fakeShadowService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FakeShadowPresenter(
            IActorEntity actorService,
            FakeShadowService fakeShadowService)
        {
            _actorEntity = actorService;
            _fakeShadowService = fakeShadowService;
        }

        void IStartable.Start()
        {
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

            _actorEntity.Active()
                .Subscribe(x => _isTick = x)
                .AddTo(_disposables);

            _fakeShadowService.Setup();
        }

        void ITickable.Tick()
        {
            if (!_isTick) return;
            _fakeShadowService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _fakeShadowService.Dispose();
            _disposables.Dispose();
        }
    }
}
