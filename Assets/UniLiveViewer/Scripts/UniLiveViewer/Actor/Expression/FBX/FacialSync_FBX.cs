using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniLiveViewer.Actor.Expression
{
    public class FacialSync_FBX : MonoBehaviour, IFacialSync
    {
        const int BLENDSHAPE_WEIGHT = 100;
        [SerializeField] SkinBindInfo[] _skinBindInfo;
        AnimationCurve _gainCurve = new();// 最初の一体のみ初期化必須

        string[] IFacialSync.GetKeyArray() => _customMap.Keys?.ToArray();
        public IReadOnlyDictionary<string, FACIALTYPE> CustomMap => _customMap;
        Dictionary<string, FACIALTYPE> _customMap = new()
        {
            //{ "ウィンク" ,FacialSyncController.FACIALTYPE.BLINK },    
            { "まばたき", FACIALTYPE.BLINK },
            { "笑い", FACIALTYPE.JOY },
            { "怒り", FACIALTYPE.ANGRY },
            { "困る", FACIALTYPE.SORROW },
            { "にやり", FACIALTYPE.FUN },
        };

        void Start()
        {
            foreach (var e in _skinBindInfo)
            {
                InitKeyPair(e);
            }
        }

        void InitKeyPair(SkinBindInfo skinBindInfo)
        {
            var blendShapeCount = skinBindInfo.skinMesh.sharedMesh.blendShapeCount;

            for (int i = 0; i < skinBindInfo.bindInfo.Length; i++)
            {
                for (int j = 0; j < skinBindInfo.bindInfo[i].keyPair.Length; j++)
                {
                    for (int n = 0; n < blendShapeCount; n++)
                    {
                        //シェイプキー名を取得
                        var shapeName = skinBindInfo.skinMesh.sharedMesh.GetBlendShapeName(n);
                        if (skinBindInfo.bindInfo[i].keyPair[j].name != shapeName) continue;
                        skinBindInfo.bindInfo[i].keyPair[j].index = n;

                        //Debug.Log($"{skinBindInfo.bindInfo[i].keyPair[j].name}:{n}");
                        break;
                    }
                }
            }
        }

        /// <param name="blendShape">使わない</param>
        /// <param name="expression">使わない</param>
        void IFacialSync.Setup(Transform parent, VRM.VRMBlendShapeProxy blendShape, UniVRM10.Vrm10RuntimeExpression expression)
        {
            if (blendShape != null) return;
            transform.SetParent(parent);
            transform.name = ActorConstants.FaceSyncController;
        }

        void IFacialSync.SetGainCurve(AnimationCurve gainCurve)
        {
            _gainCurve = gainCurve;
        }

        /// <summary>
        /// シェイプキーを更新する
        /// </summary>
        void IFacialSync.Morph()
        {
            foreach (var info in _skinBindInfo)
            {
                Morph(info);
            }
        }

        void Morph(SkinBindInfo skinBindInfo)
        {
            var total = 1.0f;
            var w = 0.0f;
            for (int i = 0; i < skinBindInfo.bindInfo.Length; i++)
            {
                w = total * GetWeight(skinBindInfo.bindInfo[i].node);
                for (int j = 0; j < skinBindInfo.bindInfo[i].keyPair.Length; j++)
                {
                    skinBindInfo.skinMesh.SetBlendShapeWeight(skinBindInfo.bindInfo[i].keyPair[j].index, w * BLENDSHAPE_WEIGHT);
                }
                total -= w;
            }
        }

        void IFacialSync.Morph(string key, float weight)
        {
            var type = _customMap[key];

            foreach (var info in _skinBindInfo)
            {
                for (int i = 0; i < info.bindInfo.Length; i++)
                {
                    if (info.bindInfo[i].facialType != type) continue;

                    for (int j = 0; j < info.bindInfo[i].keyPair.Length; j++)
                    {
                        info.skinMesh.SetBlendShapeWeight(info.bindInfo[i].keyPair[j].index, weight * BLENDSHAPE_WEIGHT);
                    }
                }
            }
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void IFacialSync.MorphReset()
        {
            foreach (var info in _skinBindInfo)
            {
                MorphReset(info);
            }
        }

        void MorphReset(SkinBindInfo skinBindInfo)
        {
            for (int i = 0; i < skinBindInfo.bindInfo.Length; i++)
            {
                for (int j = 0; j < skinBindInfo.bindInfo[i].keyPair.Length; j++)
                {
                    skinBindInfo.skinMesh.SetBlendShapeWeight(skinBindInfo.bindInfo[i].keyPair[j].index, 0);
                }
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
