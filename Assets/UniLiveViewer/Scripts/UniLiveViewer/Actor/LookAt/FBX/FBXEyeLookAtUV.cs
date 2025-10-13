using NanaCiel;
using System;
using UnityEngine;

namespace UniLiveViewer.Actor.LookAt.FBX
{
    /// <summary>
    /// UnityChan / CandyRockStar / UnityChanSD
    /// </summary>
    public class FBXEyeLookAtUV : IEyeLookAt, IDisposable
    {
        readonly int MainTexId = Shader.PropertyToID("_MainTex");

        /// <summary>
        /// 顔ベース
        /// </summary>
        const float SearchAngle_Eye = 40;

        // 手動で開放するため
        Material _eyeMat;

        float _inputWeight = 0.0f;
        Transform _lookTarget;
        /// <summary>
        /// 更新が走ってエラるのでSetup完了までは無効化
        /// </summary>
        bool _isLookAt = false;
        float _eyeLeap = 0.0f;

        readonly LookAtSettings _settings;
        readonly CharaInfoData _charaInfoData;
        readonly NormalizedBoneGenerator _test;

        public FBXEyeLookAtUV(
            LookAtSettings settings,
            CharaInfoData charaInfoData,
            NormalizedBoneGenerator normalizedBoneGenerator)
        {
            _settings = settings;
            _charaInfoData = charaInfoData;
            _test = normalizedBoneGenerator;
        }

        public void Setup(Transform lookTarget)
        {
            _lookTarget = lookTarget;

            if (_charaInfoData.ExpressionType == ExpressionType.UnityChan
                || _charaInfoData.ExpressionType == ExpressionType.CandyChan)
            {
                _eyeMat = _settings.Face.material;
            }
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
            var result = Vector3.zero;

            var v = _test.VirtualEye.InverseTransformPoint(_lookTarget.position).normalized;
            switch (_charaInfoData.ExpressionType)
            {
                case ExpressionType.UnityChan:
                case ExpressionType.CandyChan:
                    //ローカル座標に変換
                    result.x = v.x * _settings.eyeAmplitude.x * _eyeLeap;
                    result.y = -v.y * _settings.eyeAmplitude.y * _eyeLeap;
                    //UVをオフセットを反映
                    _eyeMat.SetTextureOffset(MainTexId, result);
                    break;
                case ExpressionType.UnityChanSD:
                    //ローカル座標に変換
                    result.x = -v.y * _settings.eyeAmplitude.x * _eyeLeap;
                    result.y = v.x * _settings.eyeAmplitude.y * _eyeLeap;

                    _test.LEyeAnchor.localRotation = Quaternion.Euler(new Vector3(result.x, 0, result.y));
                    _test.REyeAnchor.localRotation = Quaternion.Euler(new Vector3(result.x, 0, result.y));
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


        void IDisposable.Dispose()
        {
            if (_eyeMat) GameObject.Destroy(_eyeMat);
        }
    }
}