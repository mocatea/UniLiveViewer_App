﻿using UnityEngine;

namespace UniLiveViewer
{
    public class FacialSync_FBX : FacialSyncBase
    {
        const int BLENDSHAPE_WEIGHT = 100;

        // Start is called before the first frame update
        protected override void Start()
        {
            foreach (var e in _skinBindInfo)
            {
                InitKeyPair(e);
            }
        }

        void InitKeyPair(SkinBindInfo skinBindInfo)
        {
            int blendShapeCount = skinBindInfo.skinMesh.sharedMesh.blendShapeCount;

            for (int i = 0; i < skinBindInfo.bindInfo.Length; i++)
            {
                for (int j = 0; j < skinBindInfo.bindInfo[i].keyPair.Length; j++)
                {
                    for (int n = 0; n < blendShapeCount; n++)
                    {
                        //シェイプキー名を取得
                        string shapeName = skinBindInfo.skinMesh.sharedMesh.GetBlendShapeName(n);
                        if (skinBindInfo.bindInfo[i].keyPair[j].name != shapeName) continue;
                        skinBindInfo.bindInfo[i].keyPair[j].index = n;

                        //Debug.Log($"{skinBindInfo.bindInfo[i].keyPair[j].name}:{n}");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        public override void MorphReset()
        {
            foreach (var e in _skinBindInfo)
            {
                MorphReset(e);
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

        // Update is called once per frame
        protected override void Update()
        {
            foreach (var e in _skinBindInfo)
            {
                Morph(e);
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
    }
}