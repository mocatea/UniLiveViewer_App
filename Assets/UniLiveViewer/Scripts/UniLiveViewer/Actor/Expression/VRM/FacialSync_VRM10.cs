using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniVRM10;
using VRM;

namespace UniLiveViewer.Actor.Expression
{
    public class FacialSync_VRM10 : MonoBehaviour, IFacialSync
    {
        public Vrm10RuntimeExpression RuntimeExpression => _runtimeExpression;
        [SerializeField] Vrm10RuntimeExpression _runtimeExpression;
        AnimationCurve _gainCurve;
        readonly Dictionary<ExpressionKey, float> _map = new();

        [Header("<keyName不要>")]
        [SerializeField] SkinBindInfo[] _skinBindInfo;

        readonly Dictionary<FACIALTYPE, ExpressionPreset> _presetMap = new()
        {
            { FACIALTYPE.BLINK, ExpressionPreset.blink },
            { FACIALTYPE.JOY, ExpressionPreset.happy },
            { FACIALTYPE.ANGRY, ExpressionPreset.angry },
            { FACIALTYPE.SORROW, ExpressionPreset.sad },
            { FACIALTYPE.SUP, ExpressionPreset.oh },
            { FACIALTYPE.FUN, ExpressionPreset.relaxed },
            { FACIALTYPE.WINK_L, ExpressionPreset.blinkLeft },
            { FACIALTYPE.WINK_R, ExpressionPreset.blinkRight }
        };

        string[] IFacialSync.GetKeyArray() => _customMap.Keys?.ToArray();
        public IReadOnlyDictionary<string, ExpressionPreset> CustomMap => _customMap;
        readonly Dictionary<string, ExpressionPreset> _customMap = new()
        {   
            { "まばたき", ExpressionPreset.blink },
            { "笑い", ExpressionPreset.happy },
            { "怒り", ExpressionPreset.angry },
            { "困る", ExpressionPreset.sad },
            { "にやり", ExpressionPreset.relaxed },
            { "ウィンク左" ,ExpressionPreset.blinkLeft },
            { "ウィンク右" ,ExpressionPreset.blinkRight },
        };

        /// <param name="blendShape">使わない</param>
        void IFacialSync.Setup(Transform parent, VRMBlendShapeProxy blendShape, Vrm10RuntimeExpression expression)
        {
            if (blendShape != null) return;
            if (expression == null) return;
            _runtimeExpression = expression;
            transform.SetParent(parent);
            transform.name = ActorConstants.FaceSyncController;
        }

        void IFacialSync.SetGainCurve(AnimationCurve gainCurve)
        {
            _gainCurve = gainCurve;
        }

        void IFacialSync.Morph()
        {
            if (_runtimeExpression == null) return;
            var w = 0.0f;
            // 0許して...
            foreach (var info in _skinBindInfo[0].bindInfo)
            {
                w = GetWeight(info.node);
                var preset = _presetMap[info.facialType];
                _map[ExpressionKey.CreateFromPreset(preset)] = w;
            }
            _runtimeExpression.SetWeightsNonAlloc(_map);
        }

        void IFacialSync.Morph(string key, float weight)
        {
            var preset = _customMap[key];
            _runtimeExpression.SetWeight(ExpressionKey.CreateFromPreset(preset), weight);
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void IFacialSync.MorphReset()
        {
            if (_runtimeExpression == null) return;

            foreach (var preset in _presetMap.Values)
            {
                _map[ExpressionKey.CreateFromPreset(preset)] = 0;
            }
            _runtimeExpression.SetWeightsNonAlloc(_map);
        }

        /// <summary>
        /// モーフのバインド情報を返す
        /// </summary>
        SkinBindInfo[] IFacialSync.GetSkinBindInfo()
        {
            return _skinBindInfo;
        }

        float GetWeight(Transform tr)
        {
            return _gainCurve.Evaluate(tr.localPosition.z);
        }
    }
}
