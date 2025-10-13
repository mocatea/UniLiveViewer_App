using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.Player;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class AudioPlaybackPresenter : IAsyncStartable, ITickable, IDisposable
    {
        float _currentAudioLength = 0;

        readonly AudioPlaybackPage _audioPlaybackPage;
        readonly TimelineService _timelineService;
        readonly JumpList _jumpList;

        readonly AudioAssetManager _audioAssetManager;
        readonly PlayableDirector _playableDirector;
        readonly PlayerHandsService _playerHandsService;
        readonly RootAudioSourceService _audioSourceService;
        readonly TimelineAudioClipSwitcherService _timelineAudioClipSwitcher;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public AudioPlaybackPresenter(
            AudioPlaybackPage audioPlaybackPage,
            TimelineService timelineService,
            JumpList jumpList,
            AudioAssetManager audioAssetManager,
            PlayableDirector playableDirector,
            PlayerHandsService playerHandsService,
            TimelineAudioClipSwitcherService timelineAudioClipSwitcher,
            RootAudioSourceService audioSourceService)
        {
            _audioPlaybackPage = audioPlaybackPage;
            _timelineService = timelineService;
            _jumpList = jumpList;
            _audioAssetManager = audioAssetManager;
            _playableDirector = playableDirector;
            _playerHandsService = playerHandsService;
            _audioSourceService = audioSourceService;
            _timelineAudioClipSwitcher = timelineAudioClipSwitcher;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _audioPlaybackPage.Initialize(_audioAssetManager, _playableDirector,
                _playerHandsService, _timelineAudioClipSwitcher, _audioSourceService);

            _audioPlaybackPage.PlayAsObservable
                .Subscribe(async _ => await _timelineService.PlayAsync(cancellation))
                .AddTo(_disposables);
            _audioPlaybackPage.PauseAsObservable
                .Subscribe(async _ => await _timelineService.PauseAsync(cancellation))
                .AddTo(_disposables);
            _audioPlaybackPage.StopAsObservable
                .Subscribe(async _ => await _timelineService.StopAsync(cancellation))
                .AddTo(_disposables);

            _audioPlaybackPage.PlaybackTime
                .Subscribe(x => _timelineService.AudioClipPlaybackTime = x)
                .AddTo(_disposables);
            _audioPlaybackPage.Speed
                .Subscribe(x => _timelineService.TimelineSpeed = x)
                .AddTo(_disposables);
            _audioPlaybackPage.IsLoop
                .Subscribe(_timelineService.SetLoop)
                .AddTo(_disposables);
            _audioPlaybackPage.AudioLength
                .Subscribe(x => _currentAudioLength = x)
                .AddTo(_disposables);

            _timelineService.TimelineUpdateMode
                .Subscribe(_audioPlaybackPage.OnChangeTimelineUpdateMode)
                .AddTo(_disposables);   

            _jumpList.OnSelectAsObservable
                .Subscribe(x =>
                {
                    _audioPlaybackPage.OnJumpSelect(x, 
                        (_audioAssetManager.CurrentPreset, _audioAssetManager.CurrentCustom));
                }).AddTo(_disposables);

            _audioPlaybackPage.StartAsync(cancellation).Forget();


            await UniTask.CompletedTask;
        }

        public void Tick()
        {
            _timelineService.OnTick(_currentAudioLength);
            _audioPlaybackPage.OnTick((float)_timelineService.AudioClipPlaybackTime);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
