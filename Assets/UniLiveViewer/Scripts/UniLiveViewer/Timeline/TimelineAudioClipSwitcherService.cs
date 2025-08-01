using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VContainer;

namespace UniLiveViewer.Timeline
{
    /// <summary>
    /// PlayableDirectorの再生制御とTimelineAssetの音源操作ラッパー
    /// MEMO: 公式APIでは拡張融通が効きにくいのでラッパーが妥当、あるいはPlayableAPI
    /// </summary>
    public class TimelineAudioClipSwitcherService
    {
        readonly string[] AUDIOTRACK = {
            "Audio Track1",
            "Audio Track2",
            "Audio Track3",
            "Audio Track4"
        };

        const string MainAudioTrackAssetName = "Main Audio";

        public IObservable<AudioClip> AudioClipChangedAsObservable => _audioClipChangedStream;
        readonly Subject<AudioClip> _audioClipChangedStream = new();

        readonly TimelineService _timelineService;
        readonly AudioAssetManager _audioAssetManager;
        readonly TimelineAsset _timelineAsset;

        [Inject]
        public TimelineAudioClipSwitcherService(
            TimelineService timelineService,
            AudioAssetManager audioAssetManager,
            PlayableDirector playableDirector)
        {
            _timelineService = timelineService;
            _audioAssetManager = audioAssetManager;
            _timelineAsset = playableDirector.playableAsset as TimelineAsset;
        }

        public async UniTask BeginAsync(CancellationToken cancellation)
        {
            await SetAudioClipAsync(true, 0, cancellation);
        }

        /// <summary>
        /// 現在曲の長さ
        /// </summary>
        public async UniTask<float> GetCurrentAudioLengthAsync(bool isPreset, CancellationToken cancellation)
        {
            var AudioClip = await _audioAssetManager.TryGetCurrentAudioClipAsycn(isPreset, cancellation);
            return AudioClip == null ? 0 : AudioClip.length;
        }

        public async UniTask<string> SetAudioClipAsync(bool isPreset, int moveCurrent, CancellationToken cancellation)
        {
            var nextAudioClip = await _audioAssetManager.TryGetAudioClipAsync(cancellation, isPreset, moveCurrent);
            if (nextAudioClip == null) return null;

            var audioTracks = _timelineAsset.GetOutputTracks().OfType<AudioTrack>();
            var audioTrack = audioTracks.FirstOrDefault(x => x.name == MainAudioTrackAssetName);
            if (!audioTrack) return null;

            //トラック内のクリップを全取得
            var timelineClips = audioTrack.GetClips();
            var currentTimelineClip = timelineClips.FirstOrDefault(x => x.displayName != "");
            currentTimelineClip.duration = _timelineService.AudioClipStartTime + nextAudioClip.length;//秒

            //登録する
            (currentTimelineClip.asset as AudioPlayableAsset).clip = nextAudioClip;

            //スペクトル用
            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                if (nextAudioClip.name.Contains(".mp3") || nextAudioClip.name.Contains(".wav"))
                {
                    // NOTE: ランタイム上手くいかなかった
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        audioTrack = audioTracks.FirstOrDefault(x => x.name == AUDIOTRACK[i]);
                        timelineClips = audioTrack.GetClips();
                        currentTimelineClip = timelineClips.FirstOrDefault(x => x.displayName != "");
                        currentTimelineClip.duration = _timelineService.AudioClipStartTime + nextAudioClip.length;//秒
                        (currentTimelineClip.asset as AudioPlayableAsset).clip = nextAudioClip;
                    }
                }
            }

            _timelineService.ResumeTimeline();

            _audioClipChangedStream.OnNext(nextAudioClip);

            return nextAudioClip.name;
        }
    }
}