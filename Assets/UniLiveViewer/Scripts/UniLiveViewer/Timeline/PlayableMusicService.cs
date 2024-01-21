﻿using Cysharp.Threading.Tasks;
using MessagePipe;
using NanaCiel;
using System.Linq;
using System.Threading;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VContainer;

namespace UniLiveViewer.Timeline
{
    /// <summary>
    /// 
    /// TODO: コメントアウト部分の購読化とか
    /// </summary>
    public class PlayableMusicService
    {
        const double _motionClipStartTime = 3;//モーションクリップの開始再生位置(デフォルト)
        const string AssetNameMainAudio = "Main Audio";
        readonly string[] AUDIOTRACK = {
            "Audio Track 1",
            "Audio Track 2",
            "Audio Track 3",
            "Audio Track 4"
        };

        double _audioClipStartTime = 0;//セットされたaudioクリップの開始再生位置
        /// <summary>
        /// Timelineの再生速度
        /// </summary>
        public float TimelineSpeed
        {
            get { return _timelineSpeed; }
            set
            {
                _timelineSpeed = Mathf.Clamp(value, 0.0f, 3.0f);
                _playableDirector.SetSpeedTimeline(_timelineSpeed);
            }
        }
        float _timelineSpeed;
        double _playbackTime = 0.0f;

        IPublisher<AllActorOperationMessage> _allPublisher;
        readonly AudioAssetManager _audioAssetManager;
        readonly PlayableDirector _playableDirector;
        readonly TimelineAsset _timelineAsset;

        [Inject]
        public PlayableMusicService(
            IPublisher<AllActorOperationMessage> allPublisher,
            AudioAssetManager audioAssetManager,
            PlayableDirector playableDirector)
        {
            _allPublisher = allPublisher;
            _audioAssetManager = audioAssetManager;
            _playableDirector = playableDirector;

            _timelineAsset = _playableDirector.playableAsset as TimelineAsset;
        }

        public async UniTask OnStartAsync(CancellationToken cancellation)
        {
            TimelineSpeed = 1.0f;//起点大事

            // タイムライン内のトラック一覧を取得
            var tracks = _timelineAsset.GetOutputTracks();
            //メインオーディオのTrackAssetを取得
            var track = tracks.FirstOrDefault(x => x.name == AssetNameMainAudio);

            if (track)
            {
                //トラック内のクリップを全取得
                var clips = track.GetClips();
                // 指定名称のクリップを抜き出す
                var danceClip = clips.FirstOrDefault(x => x.displayName == "Main Audio Clip");
                //開始位置を取得
                danceClip.start = _motionClipStartTime + 2;
                _audioClipStartTime = danceClip.start;
            }
            else
            {
                Debug.Log("メインオーディオが見つかりません");
            }
            await NextAudioClip(true, 0, cancellation);
        }

        //AudioClip基準の再生時間を算出
        public double AudioClipPlaybackTime
        {
            get
            {
                _playbackTime = _playableDirector.time - _audioClipStartTime;//参考用
                return _playbackTime;
            }
            set
            {
                _playbackTime = value;
                if (_playbackTime > _playableDirector.duration) _playbackTime = _playableDirector.duration;
                _playableDirector.time = _audioClipStartTime + _playbackTime;//タイムラインに反映
            }
        }

        /// <summary>
        /// 再生状態にする
        /// </summary>
        public async UniTask PlayAsync(CancellationToken cancellation)
        {
            //表情系をリセットしておく
            //foreach (var chara in _bindCharaMap)
            //{
            //    if (chara.Value is null) continue;
            //    chara.Value.FacialSync.MorphReset();
            //    chara.Value.LipSync.MorphReset();
            //}

            //モードをマニュアルからゲームタイマーへ
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                _playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            }
            _playableDirector.ResumeTimeline();

            //後にmessage
            await UniTask.Yield(cancellation);
            var message = new AllActorOperationMessage(ActorState.NULL, ActorCommand.TIMELINE_PLAY);
            _allPublisher.Publish(message);
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
        /// 再生位置を初期化する
        /// </summary>
        public async UniTask BaseReturnAsync(CancellationToken cancellation)
        {
            _playableDirector.StopTimeline();
            AudioClipPlaybackTime = 0;

            await ManualModeAsync(cancellation);
        }

        /// <summary>
        /// 一定間隔でマニュアルモードで更新を行う
        /// </summary>
        /// <returns></returns>
        async UniTask ManualUpdateAsync(CancellationToken cancellation)
        {
            var keepVal = AudioClipPlaybackTime;

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

        /// <summary>
        /// 現在曲の長さ
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isPreset"></param>
        /// <returns></returns>
        public async UniTask<float> CurrentAudioLengthAsync(bool isPreset, CancellationToken cancellation)
        {
            var AudioClip = await _audioAssetManager.TryGetCurrentAudioClipAsycn(isPreset, cancellation);
            return AudioClip == null ? 0 : AudioClip.length;
        }

        /// <summary>
        /// 指定CurrentのBGMをセットする
        /// </summary>
        /// <param name="isPreset"></param>
        /// <param name="moveCurrent"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async UniTask<string> NextAudioClip(bool isPreset, int moveCurrent, CancellationToken cancellation)
        {
            var newAudioClip = await _audioAssetManager.TryGetAudioClipAsync(cancellation, isPreset, moveCurrent);
            if (newAudioClip == null) return "";

            var AudioTracks = _timelineAsset.GetOutputTracks().OfType<AudioTrack>();
            var AudioTrack = AudioTracks.FirstOrDefault(x => x.name == AssetNameMainAudio);
            if (!AudioTrack) return "";

            //トラック内のクリップを全取得
            var timelineClips = AudioTrack.GetClips();
            var oldAudioClip = timelineClips.FirstOrDefault(x => x.displayName != "");
            oldAudioClip.duration = _audioClipStartTime + newAudioClip.length;//秒

            //登録する
            (oldAudioClip.asset as AudioPlayableAsset).clip = newAudioClip;

            //スペクトル用
            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                if (newAudioClip.name.Contains(".mp3") || newAudioClip.name.Contains(".wav"))
                {
                    // NOTE: ランタイム上手くいかなかった
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        AudioTrack = AudioTracks.FirstOrDefault(x => x.name == AUDIOTRACK[i]);
                        timelineClips = AudioTrack.GetClips();
                        oldAudioClip = timelineClips.FirstOrDefault(x => x.displayName != "");
                        oldAudioClip.duration = _audioClipStartTime + newAudioClip.length;//秒
                        (oldAudioClip.asset as AudioPlayableAsset).clip = newAudioClip;
                    }
                }
            }
            _playableDirector.ResumeTimeline();
            return newAudioClip.name;
        }

        /// <summary>
        /// TODO: 要確認
        /// 
        /// タイムラインの変更内容を強制的?に反映させる
        /// AnimationClip変更だけ反映されないためリスタートが必要
        /// </summary>
        //void TimelineReStart()
        //{
        //    //再生時間の記録
        //    var keepTime = _playableDirector.time;
        //    ////初期化して入れ直し(これでいけちゃう謎)
        //    //_playableDirector.playableAsset = null;
        //    //_playableDirector.playableAsset = _timelineAsset;

        //    // これでいけるかも
        //    _playableDirector.RebuildGraph();

        //    //前回の続きを指定
        //    _playableDirector.time = keepTime;

        //    ////Track情報を更新する
        //    //TrackList_Update();

        //    if (_playableDirector.timeUpdateMode == DirectorUpdateMode.GameTime)
        //    {
        //        //再生
        //        _playableDirector.Play();

        //        //速度更新(Play後は再度呼び出さないとダメみたい)
        //        _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1.0f);
        //    }
        //    else if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
        //    {
        //        //更新
        //        _playableDirector.Evaluate();
        //    }
        //}
    }
}