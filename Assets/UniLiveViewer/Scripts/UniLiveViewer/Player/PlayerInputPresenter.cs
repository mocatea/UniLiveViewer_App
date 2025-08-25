using MessagePipe;
using System;
using System.Collections.Generic;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.OVRCustom;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    public class PlayerInputPresenter : IStartable, ITickable, IDisposable
    {
        /// <summary>
        /// ロード完了まで操作不可
        /// </summary>
        bool _isTick = false;

        readonly ISubscriber<PlayerInputOperationMessage> _inputOperationSubscriber;
        readonly FileAccessManager _fileAccessManager;
        readonly PlayerInputService _playerInputService;
        readonly CompositeDisposable _disposables = new();
        readonly List<OVRGrabber_UniLiveViewer> _ovrGrabbers;

        [Inject]
        public PlayerInputPresenter(
            ISubscriber<PlayerInputOperationMessage> subscriber,
            FileAccessManager fileAccessManager,
            PlayerInputService playerInputService,
            List<OVRGrabber_UniLiveViewer> ovrGrabbers)
        {
            _inputOperationSubscriber = subscriber;
            _fileAccessManager = fileAccessManager;
            _playerInputService = playerInputService;
            _ovrGrabbers = ovrGrabbers;
        }

        void IStartable.Start()
        {
            _fileAccessManager.EndLoadingAsObservable
                .Subscribe(_ => _isTick = true)
                .AddTo(_disposables);

            _inputOperationSubscriber
                .Subscribe(x => _isTick = x.IsOperable)
                .AddTo(_disposables);

            // NOTE: 改修するので雑
            // メニュー開閉時に両手を開放、デコアイテムは除外、アクター/スライダー/MenuGripperを想定
            _playerInputService.ClickMenuAsObservable()
                .Where(x => x == PlayerHandType.RHand)
                .SelectMany(_ => _ovrGrabbers)
                .Subscribe(ovrGrabber =>
                {
                    if (ovrGrabber.GrabbedObj != null && ovrGrabber.GrabbedObj.Value != null)
                    {
                        if (!ovrGrabber.GrabbedObj.Value.TryGetComponent<DecorationItemInfo>(out var decoration))
                        {
                            ovrGrabber.ForceRelease(ovrGrabber.GrabbedObj.Value);
                        }
                    }
                }).AddTo(_disposables);
        }

        void ITickable.Tick()
        {
            if (!_isTick) return;
            _playerInputService.OnTick();
        }
        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}