﻿using System.Collections.Generic;
using UnityEngine;
using VRM;
using NanaCiel;

namespace UniLiveViewer
{
    public class CharaController : MonoBehaviour
    {
        public enum CHARASTATE
        {
            NULL = 0,
            MINIATURE,
            HOLD,
            ON_CIRCLE,
            FIELD,
        }
        public enum ANIMATIONMODE
        {
            CLIP = 0,
            VMD,
        }

        public CHARASTATE GetCharaState => charaState;
        [SerializeField] private CHARASTATE charaState = CHARASTATE.NULL;
        public ANIMATIONMODE animationMode = ANIMATIONMODE.CLIP;
        public LipSyncController lipSync;
        public FacialSyncController facialSync;
        public List<VRMSpringBone> springBoneList = new List<VRMSpringBone>();//揺れもの接触判定用
        [HideInInspector]public LookAtController lookAtCon;
        public CharaInfoData charaInfoData;
        private float reScalar = 0;
        //現状VRM専用
        public IReadOnlyList<SkinnedMeshRenderer> GetSkinnedMeshRenderers;
        public IReadOnlyList<SkinnedMeshRenderer> GetMorphSkinnedMeshRenderers;
        public void SetSkinnedMeshRenderers(IReadOnlyList<SkinnedMeshRenderer> skins)
        {
            GetSkinnedMeshRenderers = skins;
            GetMorphSkinnedMeshRenderers = skins.GetMorphSkinnedMeshRenderer();
        }


        [Header("＜TimeLine自動管理＞")]
        public string bindTrackName = "";
        public RuntimeAnimatorController keepRunAnime;//アニメーション無効化の際に直前状態をキープ
        public AnimationClip keepHandL_Anime;//手アニメーション(握りから戻す際に必要)
        public AnimationClip keepHandR_Anime;//手アニメーション(握りから戻す際に必要)
        [SerializeField] private Transform overrideAnchor = null;//座標の上書き用
        private Animator animator;

        public float CustomScalar
        {
            set
            {
                reScalar = Mathf.Clamp(value, 0.25f, 20.0f);
                transform.localScale = Vector3.one * reScalar;
            }
            get
            {
                if (reScalar == 0) reScalar = SystemInfo.userProfile.data.InitCharaSize;
                return reScalar;
            }
        }

        void Awake()
        {
            animator = transform.GetComponent<Animator>();
            //Presetキャラのみ
            if (charaInfoData && charaInfoData.formatType == CharaInfoData.FORMATTYPE.FBX)
            {
                if (GetComponent<LookAtController>() != null) lookAtCon = GetComponent<LookAtController>();
            }
        }


        /// <summary>
        /// 状態設定
        /// </summary>
        /// <param name="setState"></param>
        /// <param name="overrideTarget">対象位置に座標を合わせる</param>
        public void SetState(CHARASTATE setState, Transform overrideTarget)
        {
            Vector3 globalScale = Vector3.zero;

            overrideAnchor = overrideTarget;
            charaState = setState;
            switch (charaState)
            {
                case CHARASTATE.NULL:
                    globalScale = Vector3.one;
                    gameObject.layer = SystemInfo.layerNo_Default;
                    lookAtCon.enabled = false;
                    lookAtCon.Reset_VRMLookAtEye();
                    lookAtCon.SetEnable_VRMLookAtEye(false);
                    break;
                case CHARASTATE.MINIATURE:
                    globalScale = new Vector3(0.26f, 0.26f, 0.26f);
                    gameObject.layer = SystemInfo.layerNo_GrabObject;
                    break;
                case CHARASTATE.HOLD:
                    globalScale = new Vector3(0.26f, 0.26f, 0.26f);
                    break;
                case CHARASTATE.ON_CIRCLE:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case CHARASTATE.FIELD:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    gameObject.layer = SystemInfo.layerNo_FieldObject;
                    break;
            }

            //座標の上書き設定
            if (overrideAnchor)
            {
                transform.parent = overrideAnchor;

                //親Scaleの影響を無視する為に算出
                Vector3 scr = overrideAnchor.lossyScale;
                scr.x = 1 / scr.x;
                scr.y = 1 / scr.y;
                scr.z = 1 / scr.z;

                globalScale.x *= scr.x;
                globalScale.y *= scr.y;
                globalScale.z *= scr.z;
            }
            else transform.parent = null;

            if(charaState == CHARASTATE.MINIATURE || charaState == CHARASTATE.HOLD) transform.localScale = globalScale;
            else transform.localScale = globalScale * CustomScalar;
        }

        // Update is called once per frame
        void Update()
        {
            //座標の上書き
            if (overrideAnchor)
            {
                transform.position = overrideAnchor.position;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            //揺れもの接触振動
            if (charaInfoData && charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                foreach (var springBone in springBoneList)
                {
                    if (springBone.isHit_Any)
                    {
                        if (springBone.isLeft_Any) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, charaInfoData.power, charaInfoData.time);
                        if (springBone.isRight_Any) PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, charaInfoData.power, charaInfoData.time);
                        break;
                    }
                }
            }
        }

        public void SetEnabelSpringBones(bool isEnabel)
        {
            foreach (var e in springBoneList)
            {
                e.enabled = isEnabel;
            }
        }

        /// <summary>
        /// アニメーションコントローラーをKeepし、解除する
        /// </summary>
        public void RemoveRunAnime()
        {
            if (keepRunAnime) return;
            keepRunAnime = animator.runtimeAnimatorController;
            animator.runtimeAnimatorController = null;
        }

        /// <summary>
        /// 解除したアニメーションコントローラーを元に戻す
        /// </summary>
        public void ReturnRunAnime()
        {
            if (!keepRunAnime) return;
            animator.runtimeAnimatorController = keepRunAnime;
            keepRunAnime = null;
        }
    }
}