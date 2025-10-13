using System;
using System.Collections.Generic;
using System.Linq;
using UniLiveViewer.Actor;
using UniLiveViewer.ValueObject;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VContainer;

namespace UniLiveViewer.Timeline
{
    /// <summary>
    /// TimelineのAnimationClip紐づけのみを管理するservice
    /// </summary>
    public class PlayableAnimationClipService
    {
        const string PortalTrackNames = "Animator Track_Portal";

        const string MAINCLIP = "DanceBase";
        const string SUBCLIP0 = "HandExpression";
        const string SUBCLIP1 = "HandExpression";
        const string SUBCLIP2 = "FaceClip";
        const string SUBCLIP3 = "LipClip";

        /// <summary>
        /// HandL用
        /// </summary>
        const string SUBTRACK0 = "Override 0";
        /// <summary>
        /// HandR用
        /// </summary>
        const string SUBTRACK1 = "Override 1";
        /// <summary>
        /// Face用
        /// </summary>
        const string SUBTRACK2 = "Override 2";
        /// <summary>
        /// Lip用
        /// </summary>
        const string SUBTRACK3 = "Override 3";

        /// <summary>
        /// モーションクリップの開始再生位置(デフォルト)
        /// </summary>
        const double MotionClipStartTime = 3;

        /// <summary>
        /// 新規バインド
        /// </summary>
        public IObservable<Unit> NewBindingAsObservable => _newBindingStream;
        readonly Subject<Unit> _newBindingStream = new();

        /// <summary>
        /// 別トラックにバインド
        /// （現状はPortal→Stageのみ想定）
        /// </summary>
        public IObservable<Unit> BindingToAsObservable => _bindingToStream;
        readonly Subject<Unit> _bindingToStream = new();

        Dictionary<string, DanceInfoData> _map = new();

        readonly PlayableBinderService _playableBinderService;
        readonly TimelineService _timelineService;
        readonly PresetResourceData _presetResourceData;
        readonly TimelineAsset _timelineAsset;

        [Inject]
        public PlayableAnimationClipService(
            PlayableBinderService playableBinderService,
            TimelineService timelineService,
            PresetResourceData presetResourceData,
            PlayableDirector playableDirector)
        {
            _playableBinderService = playableBinderService;
            _timelineService = timelineService;
            _presetResourceData = presetResourceData;
            _timelineAsset = playableDirector.playableAsset as TimelineAsset;
        }

        /// <summary>
        /// 新規アニメーションクリップをバインドする(※ポータル限定)
        /// NOTE: 全く切り替わらなくなったらEditorでトラックごと作り直したら直った...
        /// </summary>
        /// <param name="danceInfoData"></param>
        public void BindingNewClips(DanceInfoData danceInfoData)
        {
            var data = _playableBinderService.BindingData[TimelineConstants.PortalIndex];
            if (data == null)
            {
                Debug.LogWarning("Not originally bind");
                return;
            }

            if (!TryGetAnimationTrack<AnimationTrack>(_timelineAsset, PortalTrackNames, out var animationTrack))
            {
                Debug.Log("System : PlayableBinding名が不一致");
                return;
            }

            SetupBaseAnimationClip(animationTrack, danceInfoData, Vector3.zero, Vector3.zero);
            SetupOverrideAnimationClip(data.InstanceId, animationTrack, danceInfoData);

            _map[data.StreamName] = danceInfoData;

            _timelineService.ResumeTimeline();
            _newBindingStream.OnNext(Unit.Default);
        }

        /// <summary>
        /// TimelineAsset内から指定型TのTrackAssetを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timelineAsset"></param>
        /// <param name="targetName"></param>
        /// <param name="animationTrack"></param>
        /// <returns></returns>
        bool TryGetAnimationTrack<T>(TimelineAsset timelineAsset, string targetName, out T animationTrack) where T : TrackAsset
        {
            var animationTracks = timelineAsset.GetOutputTracks().OfType<T>();
            animationTrack = animationTracks.FirstOrDefault(x => x.name == targetName);
            if (!animationTrack) return false;
            return true;
        }

        /// <summary>
        /// 基本のダンス設定
        /// </summary>
        /// <param name="animationTrack"></param>
        /// <param name="danceInfoData"></param>
        /// <param name="initPos"></param>
        /// <param name="initEulerAngles"></param>
        void SetupBaseAnimationClip(AnimationTrack animationTrack, DanceInfoData danceInfoData, Vector3 initPos, Vector3 initEulerAngles)
        {
            var baseDanceClip = GetTimelineClip(animationTrack, MAINCLIP);
            baseDanceClip.start = MotionClipStartTime + danceInfoData.OffsetTime;
            var animationPlayableAsset = baseDanceClip.asset as AnimationPlayableAsset;
            animationPlayableAsset.clip = danceInfoData.IsReverse ?
                danceInfoData.ReversedBaseDanceClip : danceInfoData.BaseDanceClip;

            animationPlayableAsset.position = initPos;
            animationPlayableAsset.rotation = Quaternion.Euler(initEulerAngles);
        }

        /// <summary>
        /// AnimationTrack内のTimelineClipから指定Clipのみを取得
        /// （timeline設定がちゃんとしているものとしてnullは考慮しない）
        /// </summary>
        /// <param name="animationTrack"></param>
        /// <param name="targetName"></param>
        /// <returns></returns>
        TimelineClip GetTimelineClip(AnimationTrack animationTrack, string targetName)
        {
            var timelineClips = animationTrack.GetClips();
            return timelineClips.FirstOrDefault(x => x.displayName == targetName);
        }

        /// <summary>
        /// オーバーライドアニメーション設定
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="parentTrack"></param>
        /// <param name="danceInfoData"></param>
        void SetupOverrideAnimationClip(InstanceId instanceId, AnimationTrack parentTrack, DanceInfoData danceInfoData)
        {
            if (!TryGetBindingData(instanceId, out var data))
            {
                Debug.LogWarning("Not originally bind");
                return;
            }

            if (!TryGetAnimationTrack<AnimationTrack>(_timelineAsset, PortalTrackNames, out var animationTrack))
            {
                Debug.Log("System : PlayableBinding名が不一致");
                return;
            }

            var subAnimationTracks = parentTrack.GetChildTracks().OfType<AnimationTrack>();
            foreach (var subAnimationTrack in subAnimationTracks)
            {
                SetOverrideAnimationClip(subAnimationTrack, danceInfoData, data.ActorEntity.ActorEntity().Value);
            }
        }

        bool TryGetBindingData(InstanceId instanceId, out BindingData bindingData)
        {
            bindingData = _playableBinderService.BindingData.Where(x => x?.InstanceId == instanceId).FirstOrDefault();
            if (bindingData == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// オーバーライドアニメーション(上からlHand/rHand/face/lip)
        /// </summary>
        /// <param name="subAnimationTrack"></param>
        /// <param name="danceInfoData"></param>
        /// <param name="actorEntity"></param>
        void SetOverrideAnimationClip(AnimationTrack subAnimationTrack, DanceInfoData danceInfoData, ActorEntity actorEntity)
        {
            switch (subAnimationTrack.name)
            {
                case SUBTRACK0:
                    var leftHandClip = GetTimelineClip(subAnimationTrack, SUBCLIP0);
                    leftHandClip.start = MotionClipStartTime + danceInfoData.OffsetTime;
                    {
                        var animationPlayableAsset = leftHandClip.asset as AnimationPlayableAsset;
                        animationPlayableAsset.clip = danceInfoData.IsReverse ?
                            danceInfoData.ReversedOverrideHandClip : danceInfoData.OverrideHandClip;
                    }
                    break;
                case SUBTRACK1:
                    var rightHandClip = GetTimelineClip(subAnimationTrack, SUBCLIP1);
                    rightHandClip.start = MotionClipStartTime + danceInfoData.OffsetTime;
                    {
                        var animationPlayableAsset = rightHandClip.asset as AnimationPlayableAsset;
                        animationPlayableAsset.clip = danceInfoData.IsReverse ?
                            danceInfoData.ReversedOverrideHandClip : danceInfoData.OverrideHandClip;
                    }
                    break;
                case SUBTRACK2:
                    var faceClip = GetTimelineClip(subAnimationTrack, SUBCLIP2);
                    (faceClip.asset as AnimationPlayableAsset).clip = danceInfoData.OverridefacialSyncClip;
                    faceClip.start = MotionClipStartTime + danceInfoData.OffsetTime;
                    break;
                case SUBTRACK3:
                    var lipClip = GetTimelineClip(subAnimationTrack, SUBCLIP3);
                    (lipClip.asset as AnimationPlayableAsset).clip = danceInfoData.OverrideLipSyncClip;
                    lipClip.start = MotionClipStartTime + danceInfoData.OffsetTime;
                    break;
            }
        }

        /// <summary>
        /// Editor拡張のDebug用
        /// </summary>
        public void EditorOnly_SetAnimationClip(InstanceId instanceId, Vector3 pos, Vector3 eulerAngles)
        {
            TrySwitchAnimationClip(instanceId, pos, eulerAngles);
        }

        /// <summary>
        /// ポータル枠にバインドされている各種AnimationClipをActorがバインドされているトラックにコピーする
        /// PlayableBinderService側でactor(=InstanceId)が移動済みを前提とする(雑)
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="initPos"></param>
        /// <param name="initEulerAngles"></param>
        /// <returns></returns>
        public bool TrySwitchAnimationClip(InstanceId instanceId, Vector3 initPos, Vector3 initEulerAngles)
        {
            if (!TryGetBindingData(instanceId, out var data))
            {
                Debug.LogWarning("Not originally bind");
                return false;
            }

            if (!TryGetAnimationTrack<AnimationTrack>(_timelineAsset, data.StreamName, out var animationTrack))
            {
                Debug.Log("System : PlayableBinding名が不一致");
                return false;
            }

            if (!TryGetDanceInfoData(PortalTrackNames, out var customDanceInfoData))
            {
                return false;
            }

            SetupBaseAnimationClip(animationTrack, customDanceInfoData, initPos, initEulerAngles);
            SetupOverrideAnimationClip(instanceId, animationTrack, customDanceInfoData);

            //presetな状態が必要なのでcustomDanceInfoDataを使わない
            _map[data.StreamName] = _map[TimelineConstants.TrackNames[TimelineConstants.PortalIndex]];

            _timelineService.ResumeTimeline();
            _bindingToStream.OnNext(Unit.Default);
            return true;
        }

        /// <summary>
        /// ポータルにバインドされているサブトラック情報を取得
        /// presetのDanceInfoDataを使わない理由は、既に握りclipが適用されている可能性があるので引き継ぐ必要がある
        /// </summary>
        /// <param name="baseAniTrackName"></param>
        bool TryGetDanceInfoData(string baseAniTrackName, out DanceInfoData danceInfoData)
        {
            danceInfoData = new DanceInfoData();

            if (!TryGetAnimationTrack<AnimationTrack>(_timelineAsset, baseAniTrackName, out var animationTrack))
            {
                Debug.Log("System : PlayableBinding名が不一致");
                return false;
            }

            float offsetTime;
            AnimationClip baseDanceClip = null;
            AnimationClip reversedBaseDanceClip = null;
            AnimationClip overrideHandClip = null;
            AnimationClip reversedOverrideHandClip = null;
            AnimationClip overridefacialSyncClip = null;
            AnimationClip overrideLipSyncClip = null;

            var baseDanceTimelineClip = GetTimelineClip(animationTrack, MAINCLIP);
            var animationPlayableAsset = baseDanceTimelineClip.asset as AnimationPlayableAsset;
            baseDanceClip = animationPlayableAsset.clip;
            reversedBaseDanceClip = animationPlayableAsset.clip;//生成後は切り替わらないが、空だと困りそうなので同じ割り当て
            offsetTime = (float)(baseDanceTimelineClip.start - MotionClipStartTime);

            var subAnimationTracks = animationTrack.GetChildTracks().OfType<AnimationTrack>();
            foreach (var subTrack in subAnimationTracks)
            {
                switch (subTrack.name)
                {
                    case SUBTRACK0:
                        var leftHandClip = GetTimelineClip(subTrack, SUBCLIP0);
                        overrideHandClip = (leftHandClip.asset as AnimationPlayableAsset).clip;
                        reversedOverrideHandClip = danceInfoData.OverrideHandClip;//reverseか特定できないので
                        break;
                    case SUBTRACK1:
                        var rightHandClip = GetTimelineClip(subTrack, SUBCLIP1);
                        overrideHandClip = (rightHandClip.asset as AnimationPlayableAsset).clip;
                        reversedOverrideHandClip = danceInfoData.OverrideHandClip;//reverseか特定できないので
                        break;
                    case SUBTRACK2:
                        var FaceClip = GetTimelineClip(subTrack, SUBCLIP2);
                        overridefacialSyncClip = (FaceClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK3:
                        var lipClip = GetTimelineClip(subTrack, SUBCLIP3);
                        overrideLipSyncClip = (lipClip.asset as AnimationPlayableAsset).clip;
                        break;
                }
            }

            danceInfoData.Setup(offsetTime, baseDanceClip, reversedBaseDanceClip,
                overrideHandClip, reversedOverrideHandClip, overridefacialSyncClip, overrideLipSyncClip);

            return true;
        }

        /// <summary>
        /// 指定キャラの手の状態を切り替える
        /// 
        /// TODO: Handキャッシュどうする
        /// </summary>
        /// <param name="isGrabHand">握り状態にするか</param>
        public void SetHandAnimation(InstanceId instanceId, HumanBodyBones humanBodyBones, bool isGrabHand)
        {
            if (!TryGetBindingData(instanceId, out var data))
            {
                Debug.LogWarning("Not originally bind");
                return;
            }

            ////重複排除 TODO:一旦無視
            //if (isLeft)
            //{
            //    if (!isGrabHand && !charaCon.CachedClip_handL) return;
            //    else if (isGrabHand && charaCon.CachedClip_handL) return;
            //}
            //else
            //{
            //    if (!isGrabHand && !charaCon.CachedClip_handR) return;
            //    else if (isGrabHand && charaCon.CachedClip_handR) return;
            //}

            if (!TryGetAnimationTrack<AnimationTrack>(_timelineAsset, data.StreamName, out var animationTrack))
            {
                Debug.Log("System : PlayableBinding名が不一致");
                return;
            }

            var subAnimationTracks = animationTrack.GetChildTracks().OfType<AnimationTrack>();
            //var actorEntity = data.ActorService.ActorEntity().Value;

            if (humanBodyBones == HumanBodyBones.LeftHand)
            {
                var leftHandTrack = subAnimationTracks.Where(x => x.name == SUBTRACK0).FirstOrDefault();
                if (isGrabHand) SetGripHandAnimation(leftHandTrack, SUBCLIP0);
                else
                {
                    var originalDanceInfoData = _map[data.StreamName];
                    SetOriginalHandAnimation(leftHandTrack, SUBCLIP0, originalDanceInfoData);
                }
                _timelineService.ResumeTimeline();
            }
            else if (humanBodyBones == HumanBodyBones.RightHand)
            {
                var rightHandTrack = subAnimationTracks.Where(x => x.name == SUBTRACK1).FirstOrDefault();
                if (isGrabHand) SetGripHandAnimation(rightHandTrack, SUBCLIP1);
                else
                {
                    var originalDanceInfoData = _map[data.StreamName];
                    SetOriginalHandAnimation(rightHandTrack, SUBCLIP1, originalDanceInfoData);
                }
                _timelineService.ResumeTimeline();
            }
        }

        /// <summary>
        /// 指定TimelineClipに握りハンドAnimationを設定（握り状態）
        /// </summary>
        /// <param name="subAnimationTrack"></param>
        /// <param name="clipTrackName"></param>
        void SetGripHandAnimation(AnimationTrack subAnimationTrack, string clipTrackName)
        {
            var handClip = GetTimelineClip(subAnimationTrack, clipTrackName);
            // あれ読み取りだけならhandClip.animationClipでいいんじゃない？
            //animationClip = (handClip.asset as AnimationPlayableAsset).clip;
            (handClip.asset as AnimationPlayableAsset).clip = _presetResourceData.GrabHandAnimationClip;
        }

        /// <summary>
        /// 指定TimelineClipにオリジナルハンドAnimationを設定（握り解除）
        /// </summary>
        /// <param name="subAnimationTrack"></param>
        /// <param name="clipTrackName"></param>
        /// <param name="originalDanceInfoData"></param>
        void SetOriginalHandAnimation(AnimationTrack subAnimationTrack, string clipTrackName, DanceInfoData originalDanceInfoData)
        {
            var handClip = GetTimelineClip(subAnimationTrack, clipTrackName);
            (handClip.asset as AnimationPlayableAsset).clip = originalDanceInfoData.IsReverse ?
                originalDanceInfoData.ReversedOverrideHandClip : originalDanceInfoData.OverrideHandClip;
        }

        //void InternalSwitchHandType(AnimationTrack subAnimationTrack, string clipTrackName, bool isGrabHand, AnimationClip animationClip)
        //{
        //    var handClip = GetTimelineClip(subAnimationTrack, clipTrackName);
        //    //握る
        //    if (isGrabHand)
        //    {

        //        //TODO: あれ読み取りだけならhandClip.animationClipでいいんじゃない？
        //        animationClip = (handClip.asset as AnimationPlayableAsset).clip;
        //        (handClip.asset as AnimationPlayableAsset).clip = _presetResourceData.GrabHandAnimationClip;
        //    }
        //    //解除する
        //    else
        //    {
        //        (handClip.asset as AnimationPlayableAsset).clip = animationClip;
        //        animationClip = null;
        //    }
        //}
    }
}