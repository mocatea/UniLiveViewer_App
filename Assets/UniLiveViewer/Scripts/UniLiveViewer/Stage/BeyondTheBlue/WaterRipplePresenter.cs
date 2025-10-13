using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class WaterRipplePresenter : IStartable, IDisposable
    {
        readonly ISubscriber<WaterRippleMessage> _subscriber;
        readonly WaterRippleService _waterRippleService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public WaterRipplePresenter(
            ISubscriber<WaterRippleMessage> subscriber,
            WaterRippleService waterRippleService)
        {
            _subscriber = subscriber;
            _waterRippleService = waterRippleService;
        }

        void IStartable.Start()
        {
            _subscriber
                .Subscribe(x => _waterRippleService.PushRipple(x.Position))
                .AddTo(_disposable);
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}