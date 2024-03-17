﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRM;

namespace UniLiveViewer.Actor.Expression
{
    public class FacialSync_VRM : MonoBehaviour, IFacialSync
    {
        public VRMBlendShapeProxy BlendShapeProxy => _blendShapeProxy;
        [SerializeField] VRMBlendShapeProxy _blendShapeProxy;
        [SerializeField] AnimationCurve _weightCurve;

        [Header("<keyName不要>")]
        [SerializeField] SkinBindInfo[] _skinBindInfo;
        
        readonly Dictionary<FACIALTYPE, BlendShapePreset> _presetMap = new ()
        {
            {FACIALTYPE.BLINK,BlendShapePreset.Blink},
            {FACIALTYPE.JOY,BlendShapePreset.Joy},
            {FACIALTYPE.ANGRY,BlendShapePreset.Angry},
            {FACIALTYPE.SORROW,BlendShapePreset.Sorrow},
            {FACIALTYPE.SUP,BlendShapePreset.Neutral},
            {FACIALTYPE.FUN,BlendShapePreset.Fun}
        };

        string[] IFacialSync.GetKeyArray() => _customMap.Keys?.ToArray();
        public IReadOnlyDictionary<string, BlendShapePreset> CustomMap => _customMap;
        readonly Dictionary<string, BlendShapePreset> _customMap = new ()
        {
             //{ "ウィンク" ,FacialSyncController.FACIALTYPE.BLINK },    
            { "まばたき" ,BlendShapePreset.Blink },
            { "笑い" ,BlendShapePreset.Joy },
            { "怒り" ,BlendShapePreset.Angry },
            { "困る" ,BlendShapePreset.Sorrow },
            { "にやり" ,BlendShapePreset.Fun },
        };

        void IFacialSync.Setup(Transform parent, VRMBlendShapeProxy blendShape)
        {
            if (blendShape == null) return;
            _blendShapeProxy = blendShape;
            transform.SetParent(parent);
            transform.name = ActorConstants.FaceSyncController;
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
            return _weightCurve.Evaluate(tr.localPosition.z);
        }
    }
}
