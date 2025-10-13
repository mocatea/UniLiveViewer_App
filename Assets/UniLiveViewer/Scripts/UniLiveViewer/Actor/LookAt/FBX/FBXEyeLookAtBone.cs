using NanaCiel;
using UnityEngine;

namespace UniLiveViewer.Actor.LookAt.FBX
{
    /// <summary>
    /// UnityChanSSU / UnityChanKAGURA
    /// </summary>
    public class FBXEyeLookAtBone : IEyeLookAt
    {
        /// <summary>
        /// 顔ベース
        /// </summary>
        const float SearchAngle_Eye = 40;

        float _inputWeight = 0.0f;
        Transform _lookTarget;
        /// <summary>
        /// 更新が走ってエラるのでSetup完了までは無効化
        /// </summary>
        bool _isLookAt = false;
        float _eyeLeap = 0.0f;//MEMO: VMD中でも値が入ってくるのは仕様

        readonly LookAtSettings _settings;
        readonly CharaInfoData _charaInfoData;
        readonly NormalizedBoneGenerator _test;
        readonly MonoBehaviourLookAt _monoBehaviourLookAt;

        public FBXEyeLookAtBone(
            LookAtSettings settings,
            CharaInfoData charaInfoData,
            NormalizedBoneGenerator normalizedBoneGenerator,
            MonoBehaviourLookAt monoBehaviourLookAt)
        {
            _settings = settings;
            _charaInfoData = charaInfoData;
            _test = normalizedBoneGenerator;
            _monoBehaviourLookAt = monoBehaviourLookAt;
        }

        public void Setup(Transform lookTarget)
        {
            _lookTarget = lookTarget;
            _isLookAt = true;
        }

        void IEyeLookAt.SetEnable(bool isEnable)
        {
            _isLookAt = isEnable;
        }

        void IEyeLookAt.SetWeight(float weight)
        {
            _inputWeight = weight;
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_isLookAt == false) return;

            UpdateBaseEye();

            switch (_charaInfoData.ExpressionType)
            {
                case ExpressionType.UnityChanSSU:
                case ExpressionType.UnityChanKAGURA:
                    var result = _settings.eyeAmplitude.x * _eyeLeap;
                    _monoBehaviourLookAt.ChangeEyeWeight(result);
                    break;
            }
        }

        void UpdateBaseEye()
        {
            if (0.0f < _inputWeight)
            {
                //顔ベース
                var eyeAngle = Vector3.Angle(_test.VirtualHead.forward.GetHorizontalDirection(),
                    (_lookTarget.position - _test.VirtualChest.position).GetHorizontalDirection());

                if (SearchAngle_Eye > eyeAngle) _eyeLeap += Time.deltaTime * 2.0f;
                else _eyeLeap -= Time.deltaTime * 2.0f;
                _eyeLeap = Mathf.Clamp(_eyeLeap, 0.0f, _inputWeight);
            }
            else _eyeLeap = 0;//初期化
        }

        void IEyeLookAt.Reset()
        {
            // TODO
        }
    }
}