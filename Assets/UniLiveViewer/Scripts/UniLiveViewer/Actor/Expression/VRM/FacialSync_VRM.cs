using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniVRM10;
using VRM;

namespace UniLiveViewer.Actor.Expression
{
    public class FacialSync_VRM : MonoBehaviour, IFacialSync
    {
        public VRMBlendShapeProxy BlendShapeProxy => _blendShapeProxy;
        [SerializeField] VRMBlendShapeProxy _blendShapeProxy;
        AnimationCurve _gainCurve;

        [Header("<keyName不要>")]
        [SerializeField] SkinBindInfo[] _skinBindInfo;

        readonly Dictionary<FACIALTYPE, BlendShapePreset> _presetMap = new()
        {
            { FACIALTYPE.BLINK, BlendShapePreset.Blink },
            { FACIALTYPE.JOY, BlendShapePreset.Joy },
            { FACIALTYPE.ANGRY, BlendShapePreset.Angry },
            { FACIALTYPE.SORROW, BlendShapePreset.Sorrow },
            { FACIALTYPE.SUP, BlendShapePreset.Neutral },
            { FACIALTYPE.FUN, BlendShapePreset.Fun },
            { FACIALTYPE.WINK_L, BlendShapePreset.Blink_L },
            { FACIALTYPE.WINK_R, BlendShapePreset.Blink_R }
        };

        string[] IFacialSync.GetKeyArray() => _customMap.Keys?.ToArray();
        public IReadOnlyDictionary<string, BlendShapePreset> CustomMap => _customMap;
        readonly Dictionary<string, BlendShapePreset> _customMap = new()
        {
            { "まばたき", BlendShapePreset.Blink },
            { "笑い", BlendShapePreset.Joy },
            { "怒り", BlendShapePreset.Angry },
            { "困る", BlendShapePreset.Sorrow },
            { "にやり", BlendShapePreset.Fun },
            { "ウィンク左" ,BlendShapePreset.Blink_L },
            { "ウィンク右" ,BlendShapePreset.Blink_R },
        };


        /// <param name="expression">使わない</param>
        void IFacialSync.Setup(Transform parent, VRMBlendShapeProxy blendShape, Vrm10RuntimeExpression expression)
        {
            if (expression != null) return;
            if (blendShape == null) return;
            _blendShapeProxy = blendShape;
            transform.SetParent(parent);
            transform.name = ActorConstants.FaceSyncController;
        }

        void IFacialSync.SetGainCurve(AnimationCurve gainCurve)
        {
            _gainCurve = gainCurve;
        }

        void IFacialSync.Morph()
        {
            if (_blendShapeProxy == null) return;
            var total = 1.0f;
            var w = 0.0f;
            // 0許して...
            foreach (var info in _skinBindInfo[0].bindInfo)
            {
                w = total * GetWeight(info.node);
                var preset = _presetMap[info.facialType];
                _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), w);
                total -= w;
            }
        }

        void IFacialSync.Morph(string key, float weight)
        {
            var preset = _customMap[key];
            _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), weight);
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void IFacialSync.MorphReset()
        {
            if (_blendShapeProxy == null) return;

            foreach (var preset in _presetMap.Values)
            {
                _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), 0);
            }
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
