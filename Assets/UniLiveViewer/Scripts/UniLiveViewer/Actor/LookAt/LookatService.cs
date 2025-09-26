using UniLiveViewer.Actor.LookAt.FBX;
using UniLiveViewer.Actor.LookAt.VRM;
using UnityEngine;
using UniVRM10;
using VContainer;
using VRM;

namespace UniLiveViewer.Actor.LookAt
{
    public class LookAtService
    {
        public IHeadLookAt HeadLookAt => _headLookAt;
        IHeadLookAt _headLookAt;

        public IEyeLookAt EyeLookAt => _eyeLookAt;
        IEyeLookAt _eyeLookAt;

        readonly LookAtSettings _settings;
        readonly CharaInfoData _charaInfoData;
        readonly NormalizedBoneGenerator _test;

        [Inject]
        public LookAtService(
            LookAtSettings settings,
            CharaInfoData charaInfoData,
            NormalizedBoneGenerator normalizedBoneGenerator)
        {
            _settings = settings;
            _charaInfoData = charaInfoData;
            _test = normalizedBoneGenerator;
        }

        public void FBXSetup(Animator animator, Transform lookTarget)
        {
            var monoBehaviourLookAt = MonoBehaviourLookAtSetup(animator, lookTarget);
            var headLookAt = new FBXHeadLookAt(monoBehaviourLookAt, _test);
            headLookAt.Setup(lookTarget);
            _headLookAt = headLookAt;

            if (_charaInfoData.ExpressionType == ExpressionType.UnityChan
                || _charaInfoData.ExpressionType == ExpressionType.CandyChan
                || _charaInfoData.ExpressionType == ExpressionType.UnityChanSD)
            {
                var eyeLookAt = new FBXEyeLookAtUV(_settings, _charaInfoData, _test);
                eyeLookAt.Setup(lookTarget);
                _eyeLookAt = eyeLookAt;
            }
            else if (_charaInfoData.ExpressionType == ExpressionType.UnityChanSSU
                || _charaInfoData.ExpressionType == ExpressionType.UnityChanKAGURA)
            {
                var eyeLookAt = new FBXEyeLookAtBone(_settings, _charaInfoData, _test, monoBehaviourLookAt);
                eyeLookAt.Setup(lookTarget);
                _eyeLookAt = eyeLookAt;
            }
            else if (_charaInfoData.ExpressionType == ExpressionType.VketChan)
            {
                var eyeLookAt = new FBXEyeLookAtBlendShape(_settings, _charaInfoData, _test);
                eyeLookAt.Setup(lookTarget);
                _eyeLookAt = eyeLookAt;
            }
            else
            {
                _eyeLookAt = null;
            }
        }

        public void VRMSetup(Animator animator, Transform lookTarget, VRMLookAtBoneApplyer applyer)
        {
            var monoBehaviourLookAt = MonoBehaviourLookAtSetup(animator, lookTarget);
            var headLookAt = new VRMHeadLookAt(monoBehaviourLookAt, _test);
            headLookAt.Setup(lookTarget, applyer.GetComponent<VRMLookAtHead>());
            _headLookAt = headLookAt;

            var eyeLookAt = new VRMEyeLookAtBone(_charaInfoData, _test);
            eyeLookAt.Setup(lookTarget, applyer);
            _eyeLookAt = eyeLookAt;
        }

        public void VRMSetup(Animator animator, Transform lookTarget, VRMLookAtBlendShapeApplyer applyer)
        {
            var monoBehaviourLookAt = MonoBehaviourLookAtSetup(animator, lookTarget);
            var headLookAt = new VRMHeadLookAt(monoBehaviourLookAt, _test);
            headLookAt.Setup(lookTarget, applyer.GetComponent<VRMLookAtHead>());
            _headLookAt = headLookAt;

            var eyeLookAt = new VRMEyeLookAtBlendShape(_charaInfoData, _test);
            eyeLookAt.Setup(lookTarget, applyer);
            _eyeLookAt = eyeLookAt;
        }

        public void VRM10Setup(Animator animator, Transform lookTarget, Vrm10Instance vrm10Instance)
        {
            var monoBehaviourLookAt = MonoBehaviourLookAtSetup(animator, lookTarget);
            var headLookAt = new VRMHeadLookAt(monoBehaviourLookAt, _test);
            headLookAt.Setup(lookTarget);
            _headLookAt = headLookAt;

            var eyeLookAt = new VRM10EyeLookAt(_charaInfoData, _test);
            eyeLookAt.Setup(lookTarget, vrm10Instance);
            _eyeLookAt = eyeLookAt;
        }

        MonoBehaviourLookAt MonoBehaviourLookAtSetup(Animator animator, Transform lookTarget)
        {
            var monoBehaviourLookAt = animator.gameObject.AddComponent<MonoBehaviourLookAt>();
            monoBehaviourLookAt.Setup(animator, lookTarget);
            return monoBehaviourLookAt;
        }

        public void SetHeadEnable(bool isEnable)
        {
            _headLookAt.SetEnable(isEnable);
        }

        public void SetHeadWeight(float weight)
        {
            _headLookAt.SetWeight(weight);
        }

        /// <summary>
        /// 現状使われていない
        /// </summary>
        public void SetEyeEnable(bool isEnable)
        {
            _eyeLookAt.SetEnable(isEnable);
        }

        public void SetEyeWeight(float weight)
        {
            _eyeLookAt.SetWeight(weight);
        }

        public void OnLateTick()
        {
            _headLookAt?.OnLateTick();
            _eyeLookAt?.OnLateTick();
        }
    }
}
