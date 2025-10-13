using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    public class PlayableMusicPresenter : IAsyncStartable, IDisposable
    {
        readonly TimelineService _timelineService;
        readonly TimelineAudioClipSwitcherService _timelineAudioClipSwitcher;
        readonly RootAudioSourceService _audioSourceService;
        readonly SpectrumConverter _spectrumConverter;
        readonly CompositeDisposable _disposable = new();

        [Inject]
        public PlayableMusicPresenter(
            TimelineService timelineService,
            TimelineAudioClipSwitcherService timelineAudioClipSwitcher,
            RootAudioSourceService audioSourceService,
            SpectrumConverter spectrumConverter)
        {
            _timelineService = timelineService;
            _timelineAudioClipSwitcher = timelineAudioClipSwitcher;
            _audioSourceService = audioSourceService;
            _spectrumConverter = spectrumConverter;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _spectrumConverter.Initialize(_audioSourceService.BgmAudioSource);

            _timelineAudioClipSwitcher.AudioClipChangedAsObservable
                .Subscribe(_spectrumConverter.ResetSpectrumData)
                .AddTo(_disposable);

            _timelineService.Begin();
            await _timelineAudioClipSwitcher.BeginAsync(cancellation);

            OVRManager.InputFocusLost += async () => await HomePause(cancellation);
            OVRManager.InputFocusAcquired += HomeReStart;
            OVRManager.HMDUnmounted += async () => await HomePause(cancellation);//HMDが外された
            OVRManager.HMDMounted += HomeReStart;//HMDが付けられた
        }

        async UniTask HomePause(CancellationToken cancellation)
        {
            await _timelineService.PauseAsync(cancellation);
            Time.timeScale = 0;
        }

        void HomeReStart()
        {
            Time.timeScale = 1;
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}