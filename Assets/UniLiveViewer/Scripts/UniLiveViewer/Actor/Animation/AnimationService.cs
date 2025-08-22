using Cysharp.Threading.Tasks;
using System.IO;
using System.Threading;
using UniLiveViewer.External;
using UniLiveViewer.External.UnityVMDReader;
using UniLiveViewer.Menu;
using UniLiveViewer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

namespace UniLiveViewer.Actor.Animation
{
    public class AnimationService
    {
        /// <summary>
        /// 停止時Motionが死ぬのでTimelineがManual時のみ脱ぐ
        /// ここにダンスフォーマットは関係しない
        /// </summary>
        RuntimeAnimatorController _cachedAnimatorController;
        ActorEntity _actorEntity;
        CurrentMode _currentMode;

        readonly PlayableDirector _playableDirector;
        readonly PlayableAnimationClipService _playableAnimationClipService;
        readonly PresetResourceData _presetResourceData;
        readonly VMDData _vmdData;
        IVMDPlayer _vmdPlayer;

        [Inject]
        public AnimationService(
            PlayableDirector playableDirector,
            PlayableAnimationClipService playableAnimationClipService,
            RuntimeAnimatorController cachedAnimatorController,
            PresetResourceData presetResourceData,
            VMDData vmdData)
        {
            _playableDirector = playableDirector;
            _playableAnimationClipService = playableAnimationClipService;
            _cachedAnimatorController = cachedAnimatorController;
            _presetResourceData = presetResourceData;
            _vmdData = vmdData;
        }

        public void OnChangeAnimator(ActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
            if (actorEntity == null)
            {
                _vmdPlayer = null;
                return;
            }
            _vmdPlayer = actorEntity.GetVMDPlayer;

            var animator = _actorEntity.GetAnimator;
            animator.runtimeAnimatorController = _cachedAnimatorController;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            //animator.applyRootMotion = false;最早関係なさそう

            //自分で検知する
            if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;
            RemoveRuntimeAnimatorController();
        }

        public async UniTask SetAnimationAsync(CurrentMode mode, int index, bool isReverse, CancellationToken cancellation)
        {
            if (_actorEntity == null) return;

            _currentMode = mode;
            if (_currentMode == CurrentMode.PRESET)
            {
                _vmdPlayer.Stop();
                _vmdPlayer.ClearMotionAndExpressionData();
                _actorEntity.GetAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);//Animator層が動いているので
                _actorEntity.GetAnimator.enabled = true;

                // ポータル上のアニメーションバインド
                var data = _presetResourceData.DanceInfoData[index];
                data.IsReverse = isReverse;
                _playableAnimationClipService.BindingNewClips(data);
            }
            else if (_currentMode == CurrentMode.CUSTOM)
            {
                // MEMO: 常時脱いでおかないと一瞬anim側のポーズが適用されポーズ時の姿勢がブレる
                RemoveRuntimeAnimatorController();

                _actorEntity.GetAnimator.enabled = false;
                _actorEntity.GetAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);//Animator層が動いているので

                var existingVMD = _vmdData.TryGetCurrentVMD();
                var folderPath = PathsInfo.GetFullPath(FolderType.Motion) + "/";
                var fileName = _vmdData.GetCurrentName();
                if (!string.IsNullOrEmpty(fileName))
                {
                    await PlayVMDAsync(existingVMD, folderPath, fileName, true, cancellation);
                    await TrySetSyncVMDAsync(cancellation);
                }
                // 空データで実質nullバインドする
                var data = _presetResourceData.VMDDanceInfoData;
                _playableAnimationClipService.BindingNewClips(data);
            }
        }

        /// <summary>
        /// VMDを再生する
        /// TODO: いつかちゃんとする
        /// </summary>
        /// <param name="existingVMD"></param>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="isBaseMotion"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        async UniTask PlayVMDAsync(VMD existingVMD, string folderPath, string fileName, bool isBaseMotion, CancellationToken cancellation)
        {
            var isSmoothVMD = FileReadAndWriteUtility.UserProfile.IsSmoothVMD;
            var boneAmplifier = FileReadAndWriteUtility.UserProfile.VMDScale;

            //新規
            if (existingVMD == null)
            {
                var info = new VMDSetupInfo(null, folderPath, fileName, boneAmplifier, isSmoothVMD);
                VMD newVMD = null;
                if (isBaseMotion)
                {
                    newVMD = await _vmdPlayer.PlayMotionAsync(info, cancellation);
                }
                else
                {
                    newVMD = await _vmdPlayer.PlayExpressionAsync(info, cancellation);
                }
                _vmdData.Add(fileName, newVMD);
            }
            //既存は使いまわす
            else
            {
                var info = new VMDSetupInfo(existingVMD, folderPath, fileName, boneAmplifier, isSmoothVMD);
                if (isBaseMotion)
                {
                    await _vmdPlayer.PlayMotionAsync(info, cancellation);
                }
                else
                {
                    await _vmdPlayer.PlayExpressionAsync(info, cancellation);
                }
            }
        }

        public void Stop()
        {
            _vmdPlayer.Stop();
        }

        public bool IsPlaying()
        {
           return _vmdPlayer.IsPlaying();
        }

        public async UniTask OnChangeScale(CancellationToken cancellation)
        {
            if (_currentMode != CurrentMode.CUSTOM) return;
            await _vmdPlayer.ReplayMotionAsync(cancellation);
            await _vmdPlayer.ReplayExpressionAsync(cancellation);
        }

        async UniTask TrySetSyncVMDAsync(CancellationToken cancellation)
        {
            if (_currentMode != CurrentMode.CUSTOM) return;
            if (!_vmdData.TryGetCurrentSyncName(out string syncFileName)) return;

            if (syncFileName == TimelineConstants.NoCustomFacialSyncMessage)
            {
                _vmdPlayer.ClearExpressionData();
            }
            else
            {
                var folderPath = PathsInfo.GetFacialSyncFolderPath() + "/";
                var fullPath = folderPath + syncFileName;
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"file does not exist:{fullPath}");
                    return;
                }

                var vmd = _vmdData.TryGetCurrentSyncVMD(syncFileName);
                await PlayVMDAsync(vmd, folderPath, syncFileName, false, cancellation);
            }
        }

        public void OnLateTick()
        {
            if (_actorEntity == null) return;
            _vmdPlayer.ToeIKReset();
        }

        public void TryVMDInitializePose()
        {
            if (_actorEntity == null) return;
            if (_currentMode != CurrentMode.CUSTOM) return;
            //_vmdPlayer.Stop(); 本来停止が正しいが、マニュアルモード中スライダー同期しなくなるのでしない
            _vmdPlayer.InitializePose();
        }

        /// <summary>
        /// アニメーションコントローラーをKeepし、解除する
        /// </summary>
        public void RemoveRuntimeAnimatorController()
        {
            if (_actorEntity == null) return;
            if (!_actorEntity.GetAnimator.runtimeAnimatorController) return;
            _cachedAnimatorController = _actorEntity.GetAnimator.runtimeAnimatorController;
            _actorEntity.GetAnimator.runtimeAnimatorController = null;
        }

        /// <summary>
        /// 解除したアニメーションコントローラーを元に戻す
        /// </summary>
        public void ReturnRuntimeAnimatorController()
        {
            if (_actorEntity == null) return;
            if (_currentMode == CurrentMode.CUSTOM) return;// 不都合回避の為VMDは常時脱いだまま
            if (!_cachedAnimatorController) return;
            _actorEntity.GetAnimator.runtimeAnimatorController = _cachedAnimatorController;
            _cachedAnimatorController = null;
        }
    }
}
