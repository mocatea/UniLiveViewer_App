﻿using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// TODO: LS化する
    /// </summary>
    public class AutoCamera : MonoBehaviour
    {
        enum SWITCHTYPE
        {
            ALL,
            RANDOM_ONE,
            RANDOM_BACKIMAGE
        }

        public bool isUpdate = true;
        [SerializeField] Transform target;
        [SerializeField] Transform baseTransform;
        [Header("＜parameters＞")]
        [SerializeField] int interval = 5000;
        [SerializeField] float offsetUp, offsetDown, offsetRight, offsetLeft;
        [SerializeField] SWITCHTYPE switchType = SWITCHTYPE.ALL;//カメラ候補を切り替えるモード
        [SerializeField] Camera[] _camera;
        [SerializeField] SpriteRenderer[] _spr;
        CancellationToken _cancellationToken;
        PlayableBinderService _playableBinderService;

        void Start()
        {
            // NOTE: globalにchara[]取れないと厳しいので配下にするか、ユニーク設定はinterface
            var container = LifetimeScope.Find<TimelineLifetimeScope>().Container;
            _playableBinderService = container.Resolve<PlayableBinderService>();
            _cancellationToken = this.GetCancellationTokenOnDestroy();

            if (switchType != SWITCHTYPE.ALL)
            {
                _playableBinderService.BindingToAsObservable
                    .Subscribe(_ => OnUpdate())
                    .AddTo(this);
            }

            foreach (var e in _camera)
            {
                e.enabled = false;
            }

            AutoUpdate().Forget();
        }

        /// <summary>
        /// ポータル以外で一番若いindexキャラを被写体に設定
        /// </summary>
        void OnUpdate()
        {
            if (target) return;
            for (int i = 0; i < _playableBinderService.BindingData.Count; i++)
            {
                if (i == TimelineConstants.PortalIndex) continue;
                var data = _playableBinderService.BindingData[i];
                if (data != null)
                {
                    var boneGenerator = data.ActorEntity.ActorEntity().Value.NormalizedBoneGenerator;
                    target = boneGenerator.VirtualHead;
                    baseTransform = boneGenerator.VirtualChest;
                    break;
                }
            }
        }

        async UniTask AutoUpdate()
        {
            int r = 0;
            Vector3 pos;

            while (true)
            {
                await UniTask.Delay(interval, cancellationToken: _cancellationToken);
                if (!isUpdate) continue;

                if (target && baseTransform)
                {
                    pos = baseTransform.position + (baseTransform.forward * 2);
                    pos += (baseTransform.up * Random.Range(-offsetDown, offsetUp)) + (baseTransform.right * Random.Range(-offsetLeft, offsetRight));
                    foreach (var e in _camera)
                    {
                        e.transform.position = pos;
                        e.transform.forward = target.position - e.transform.position;
                    }
                }

                //スクリーンに一瞬反映させる
                switch (switchType)
                {
                    case SWITCHTYPE.ALL:
                        foreach (var e in _camera) e.enabled = true;
                        await UniTask.Yield(PlayerLoopTiming.Update, _cancellationToken);
                        foreach (var e in _camera) e.enabled = false;
                        break;
                    case SWITCHTYPE.RANDOM_ONE:
                        r = Random.Range(0, _camera.Length);
                        _camera[r].enabled = true;
                        await UniTask.Yield(PlayerLoopTiming.Update, _cancellationToken);
                        _camera[r].enabled = false;
                        break;
                    case SWITCHTYPE.RANDOM_BACKIMAGE:
                        r = Random.Range(0, _spr.Length);
                        for (int i = 0; i < _spr.Length; i++) _spr[i].enabled = (i == r);
                        foreach (var e in _camera) e.enabled = true;
                        await UniTask.Yield(PlayerLoopTiming.Update, _cancellationToken);
                        foreach (var e in _camera) e.enabled = false;
                        break;
                }
            }
        }
    }
}