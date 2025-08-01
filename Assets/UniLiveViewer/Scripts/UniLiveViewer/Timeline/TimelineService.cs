using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Linq;
using System.Threading;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VContainer;

namespace UniLiveViewer.Timeline
{
    /// <summary>
    /// PlayableDirectorの再生制御ラッパー
    /// MEMO: 公式APIでは拡張融通が効きにくいのでラッパーが妥当、あるいはPlayableAPI
    /// </summary>
    public class TimelineService
    {
        const double MotionClipStartTime = 3;//モーションクリップの開始再生位置(デフォルト)
        const string MainAudioTrackAssetName = "Main Audio";
        const string MainAudioDisplayName = "Main Audio Clip";

        /// <summary>
        /// Timelineの再生速度
        /// </summary>
        public float TimelineSpeed
        {
            get { return _timelineSpeed; }
            set
            {
                _timelineSpeed = Mathf.Clamp(value, 0.0f, 3.0f);
                _cacheSpeed = _timelineSpeed;
                _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);
            }
        }
        float _timelineSpeed;

        //AudioClip基準の再生時間を算出
        public double AudioClipPlaybackTime
        {
            get
            {
                _playbackTime = _playableDirector.time - _audioClipStartTime;//参考用
                return _playbackTime;
            }
            //変更時はマニュアルモードにすること
            set
            {
                if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;
                _playbackTime = value;
                if (_playbackTime > _playableDirector.duration) _playbackTime = _playableDirector.duration;
                _playableDirector.time = _audioClipStartTime + _playbackTime;//タイムラインに反映
            }
        }

        public double AudioClipStartTime => _audioClipStartTime;
        double _audioClipStartTime = 0;//セットされたaudioクリップの開始再生位置
        double _playbackTime = 0.0f;

        double _cacheSpeed;

        readonly IPublisher<AllActorOperationMessage> _allPublisher;
        readonly IPublisher<AttachPointMessage> _attachPointPublisher;
        readonly PlayableDirector _playableDirector;
        readonly TimelineAsset _timelineAsset;

        [Inject]
        public TimelineService(
            IPublisher<AllActorOperationMessage> allPublisher,
            IPublisher<AttachPointMessage> attachPointPublisher,
            PlayableDirector playableDirector)
        {
            _allPublisher = allPublisher;
            _attachPointPublisher = attachPointPublisher;
            _playableDirector = playableDirector;

            _timelineAsset = _playableDirector.playableAsset as TimelineAsset;
        }

        public void Begin()
        {
            TimelineSpeed = 1.0f;//起点大事

            // タイムライン内のトラック一覧を取得
            var tracks = _timelineAsset.GetOutputTracks();
            //メインオーディオのTrackAssetを取得
            var track = tracks.FirstOrDefault(x => x.name == MainAudioTrackAssetName);

            if (track)
            {
                //トラック内のクリップを全取得
                var clips = track.GetClips();
                // 指定名称のクリップを抜き出す
                var danceClip = clips.FirstOrDefault(x => x.displayName == MainAudioDisplayName);
                //開始位置を取得
                danceClip.start = MotionClipStartTime + 2;
                _audioClipStartTime = danceClip.start;
            }
            else
            {
                Debug.Log("メインオーディオが見つかりません");
            }
        }

        /// <summary>
        /// 再生状態にする
        /// </summary>
        public async UniTask PlayAsync(CancellationToken cancellation)
        {
            //モードをマニュアルからゲームタイマーへ
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                _playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            }
            ResumeTimeline();

            //後にmessage
            await UniTask.Yield(cancellation);
            var allActorOperationMessage = new AllActorOperationMessage(ActorState.NULL, ActorCommand.TIMELINE_PLAY);
            _allPublisher.Publish(allActorOperationMessage);
            var attachPointMessage = new AttachPointMessage(false);
            _attachPointPublisher.Publish(attachPointMessage);
        }

        /// <summary>
        /// 再生位置を初期化する
        /// </summary>
        public async UniTask BaseReturnAsync(CancellationToken cancellation)
        {
            _playableDirector.Stop();//停止状態にする(UIにトリガーを送る為)

            await ManualModeAsync(cancellation);
            AudioClipPlaybackTime = 0;
        }

        /// <summary>
        /// マニュアル状態にする
        /// </summary>
        public async UniTask ManualModeAsync(CancellationToken cancellation)
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) return;

            //先にmessage
            var message = new AllActorOperationMessage(ActorState.NULL, ActorCommand.TIMELINE_NONPLAY);
            _allPublisher.Publish(message);
            await UniTask.Yield(cancellation);

            //マニュアルモードに
            _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;

            //マニュアルモードでの更新を開始
            ManualUpdateAsync(cancellation).Forget();
        }

        /// <summary>
        /// 一定間隔でマニュアルモードで更新を行う
        /// </summary>
        async UniTask ManualUpdateAsync(CancellationToken cancellation)
        {
            var keepVal = AudioClipPlaybackTime;

            _playableDirector.Evaluate();//一度反映しておく

            while (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //更新されているか
                if (keepVal != AudioClipPlaybackTime)
                {
                    //状態を反映させる
                    _playableDirector.Evaluate();

                    //キープの更新
                    keepVal = AudioClipPlaybackTime;
                }
                await UniTask.Delay(100, cancellationToken: cancellation);
            }
        }

        public void ResumeTimeline()
        {
            //再生時間の記録
            var keepTime = _playableDirector.time;
            ////初期化して入れ直し(これでいけちゃう謎)
            //_playableDirector.playableAsset = null;
            //_playableDirector.playableAsset = _timelineAsset;

            // clipこれでよさそう
            _playableDirector.RebuildGraph();

            //前回の続きを指定
            _playableDirector.time = keepTime;

            ////Track情報を更新する
            //TrackList_Update();

            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.GameTime)
            {
                _playableDirector.Play();
                _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_cacheSpeed);//Play後に再適用必須
            }
            else if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //1f更新
                _playableDirector.Evaluate();
            }
        }

        //AudioClip NowAudioClip()
        //{
        //    var audioTracks = _timelineAsset.GetOutputTracks().OfType<AudioTrack>();
        //    var audioTrack = audioTracks.FirstOrDefault(x => x.name == MainAudioTrackAssetName);
        //    if (!audioTrack) return null;

        //    //トラック内のクリップを全取得
        //    var timelineClips = audioTrack.GetClips();
        //    var audioClip = timelineClips.FirstOrDefault(x => x.displayName != "");
        //    return (audioClip.asset as AudioPlayableAsset).clip;
        //}
    }
}