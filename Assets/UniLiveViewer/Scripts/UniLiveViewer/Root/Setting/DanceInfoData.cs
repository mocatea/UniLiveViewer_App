using UnityEngine;
using UnityEngine.Localization;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/DanceInfoData", fileName = "DanceInfoData")]
    public class DanceInfoData : ScriptableObject
    {
        public enum FORMATTYPE
        {
            FBX = 0,
            VMD,
        }

        public void Setup(float offsetTime,
            AnimationClip baseDanceClip, AnimationClip reversedBaseDanceClip,
            AnimationClip overrideHandClip, AnimationClip reversedOverrideHandClip,
            AnimationClip overridefacialSyncClip, AnimationClip overrideLipSyncClip)
        {
            _offsetTime = offsetTime;
            _baseDanceClip = baseDanceClip;
            _reversedBaseDanceClip = reversedBaseDanceClip;
            _overrideHandClip = overrideHandClip;
            _reversedOverrideHandClip = reversedOverrideHandClip;
            _overridefacialSyncClip = overridefacialSyncClip;
            _overrideLipSyncClip = overrideLipSyncClip;
        }

        // MEMO: この書き換え辞めたい
        public bool IsReverse = false;

        // 使ってないメモ用
        [SerializeField] string _beforeName;

        /// <summary> 翻訳で切り替わる </summary>
        public LocalizedString DisplayName => _displayName;
        [SerializeField] LocalizedString _displayName;

        public float OffsetTime => _offsetTime;
        [SerializeField] float _offsetTime = 0;//現状FBXだけ、VMDはtxtを参照している

        [SerializeField] FORMATTYPE _formatType;

        public AnimationClip BaseDanceClip => _baseDanceClip;
        [SerializeField] AnimationClip _baseDanceClip;

        public AnimationClip ReversedBaseDanceClip => _reversedBaseDanceClip;
        [SerializeField] AnimationClip _reversedBaseDanceClip;

        public AnimationClip OverrideHandClip => _overrideHandClip;
        [SerializeField] AnimationClip _overrideHandClip;

        public AnimationClip ReversedOverrideHandClip => _reversedOverrideHandClip;
        [SerializeField] AnimationClip _reversedOverrideHandClip;

        public AnimationClip OverridefacialSyncClip => _overridefacialSyncClip;
        [SerializeField] AnimationClip _overridefacialSyncClip;

        public AnimationClip OverrideLipSyncClip => _overrideLipSyncClip;
        [SerializeField] AnimationClip _overrideLipSyncClip;

        public AnimationCurve FacialSyncGainCurve => _facialSyncGainCurve;
        [SerializeField] AnimationCurve _facialSyncGainCurve;
        public AnimationCurve LipSyncGainCurve => _lipSyncGainCurve;
        [SerializeField] AnimationCurve _lipSyncGainCurve;
    }
}