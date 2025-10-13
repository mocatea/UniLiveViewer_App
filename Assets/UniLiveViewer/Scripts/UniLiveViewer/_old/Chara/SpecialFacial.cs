using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using NanaCiel;

namespace UniLiveViewer
{
    //今実装中止中
    [RequireComponent(typeof(AudioSource))]
    public class SpecialFacial : MonoBehaviour
    {
        /*

        [Header("＜旧Unityちゃん用＞")]
        //[SerializeField]
        //private SkinnedMeshRenderer face;
        //[SerializeField]
        //private SkinnedMeshRenderer faceTrans;
        private int[] listFaceID;
        private int[] listFaceTransID;

        private bool isAngFace = false;

        //[Header("＜仮想ヘッド用＞")]
        //[SerializeField]
        //private string myLayer = "VirtualHead";
        //private Transform virtualHead;

        //private CharaController charaCon;
        //private LookAtController lookAtCon;
        private bool faceChanging = false;
        private Vector3 dir;
        private RaycastHit rayHit;
        private bool isShockSound = false;

        private AudioSource audioSource;
        [SerializeField]
        private AudioClip[] Sound;
        [SerializeField]
        private AudioClip[] Sound_ANG;
        [SerializeField]
        private AudioClip[] Sound_CONF;

        */

        //private void Awake()
        //{
        //    audioSource = GetComponent<AudioSource>();
        //    audioSource.volume = SystemInfo.soundVolume_SE;
        //    lookAtCon = GetComponent<LookAtController>();

        //    charaCon = GetComponent<CharaController>();
        //    if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.UnityChan)
        //    {
        //        //ジト目50% + (CONF100% or ANG100%)
        //        listFaceID = new int[3] { 17, 3, 4 };
        //        listFaceTransID = new int[3] { 6, 10, 12 };
        //    }
        //    else if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.CandyChan)
        //    {
        //        listFaceID = new int[3] { 17, 3, 4 };
        //        listFaceTransID = new int[3] { 12, 3, 5 };
        //    }
        //}

        //// Start is called before the first frame update
        //void Start()
        //{
        //    audioSource.playOnAwake = false;
        //    //virtualHead = new GameObject("VirtualHead").transform;
        //    //virtualHead.parent = transform;
        //    //virtualHead.gameObject.layer = LayerMask.NameToLayer(myLayer);
        //    //virtualHead.gameObject.AddComponent(typeof(SphereCollider));
        //    //var col = virtualHead.GetComponent<SphereCollider>();
        //    //col.radius = 0.06f;
        //    //col.isTrigger = true;

        //    //初期は無効化しておき、設置状態のみ有効化
        //    this.enabled = false;
        //}

        //// Update is called once per frame
        //void Update()
        //{
        //    //マニュアルモード中のAnimeConがkeep中のみ
        //    if (!charaCon.keepRunAnime) return;

        //    //Lookatが有効状態の時
        //    if (charaCon.lookAtCon.inputWeight_Eye <= 0.0f) return;

        //    ////仮ヘッドをキャラの頭に合わせる
        //    //virtualHead.position = CharaCon.centerEyeAnchor.position;
        //    //virtualHead.rotation = CharaCon.lastSpineAnchor.rotation;

        //    if (!faceChanging)
        //    {
        //        //ターゲットが一定より低く、近接状態か
        //        if ((lookAtCon.lookTarget.position.y - transform.position.y) > 1.1f) return;
        //        if ((transform.position - lookAtCon.lookTarget.position).GetHorizontalDirection().sqrMagnitude < 0.4f)
        //        {
        //            //ローカル座標に変換
        //            dir = lookAtCon.virtualHead.InverseTransformPoint(lookAtCon.lookTarget.position).normalized;
        //            //仮ヘッドから見てターゲットを見下ろす状態
        //            if (dir.y < -0.55f && dir.z >= 0.1f)
        //            {
        //                //表情変更
        //                if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.UnityChan
        //                    || charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.CandyChan)
        //                {
        //                    StartCoroutine(ChangeFace());
        //                }
        //                else if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.VRM_BlendShape
        //                    || charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.VRM_Bone
        //                    || charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.VRM_UV)
        //                {
        //                    StartCoroutine(ChangeFace_VRM());
        //                }
        //                //不満音
        //                audioSource.PlayOneShot(Sound[0]);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //ターゲットがこちらに気づいていなければ(ショック音がまだ)
        //        if (!isShockSound)
        //        {
        //            //ターゲットから視線のrayを飛ばす
        //            Physics.Raycast(lookAtCon.lookTarget.position, lookAtCon.lookTarget.forward, out rayHit, 2.0f, SystemInfo.layerMask_VirtualHead);
        //            //仮ヘッドにヒットしていれば(ターゲットがこちらを見た)
        //            if (rayHit.collider && rayHit.collider.transform.root == transform)
        //            {
        //                isShockSound = true;

        //                if (isAngFace)
        //                {
        //                    //ANG音
        //                    int i = Random.Range(0, Sound_ANG.Length);
        //                    audioSource.PlayOneShot(Sound_ANG[i]);
        //                }
        //                else
        //                {
        //                    //CONF音
        //                    int i = Random.Range(0, Sound_CONF.Length);
        //                    audioSource.PlayOneShot(Sound_CONF[i]);
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// VRM用・・・とりあえず
        ///// </summary>
        //public void SetAudioClip_VRM(AudioClip[] clips)
        //{
        //    Sound = new AudioClip[1];
        //    Sound[0] = clips[0];

        //    Sound_ANG = new AudioClip[2];
        //    Sound_ANG[0] = clips[1];
        //    Sound_ANG[1] = clips[2];

        //    Sound_CONF = new AudioClip[4];
        //    Sound_CONF[0] = clips[3];
        //    Sound_CONF[1] = clips[4];
        //    Sound_CONF[2] = clips[5];
        //    Sound_CONF[3] = clips[6];
        //}

        ///// <summary>
        ///// 表情遷移(旧Unityちゃん用)
        ///// </summary>
        ///// <returns></returns>
        //private IEnumerator ChangeFace()
        //{
        //    //Conf
        //    int faceID = listFaceID[1];
        //    int faceTransID = listFaceTransID[1];
        //    isAngFace = false;
        //    if (Random.Range(0, 2) == 1)
        //    {
        //        //Ang
        //        isAngFace = true;
        //        faceID = listFaceID[2];
        //        faceTransID = listFaceTransID[2];
        //    }
        //    faceChanging = true;

        //    //競合するのでリップシンクを止める
        //    //charaCon._lipSync.enabled = false;
        //    yield return null;

        //    //既存表情を全て初期化しておく
        //    charaCon._facialSync.MorphReset();
        //    //既存表情を全て初期化しておく
        //    charaCon._lipSync.MorphReset();
        //    yield return null;

        //    float weight = 0;
        //    //顔遷移
        //    for (int i = 0; i <= 10; i++)
        //    {
        //        weight = 5 * i;
        //        //EYE_DEF_C
        //        //charaCon.facialSync.uniSkin[0].SetBlendShapeWeight(listFaceID[0], weight);
        //        ////EL_DEF_C
        //        //charaCon.facialSync.uniSkin[1].SetBlendShapeWeight(listFaceTransID[0], weight);

        //        //weight = 10 * i;
        //        ////random
        //        //charaCon.facialSync.uniSkin[0].SetBlendShapeWeight(faceID, weight);
        //        //charaCon.facialSync.uniSkin[1].SetBlendShapeWeight(faceTransID, weight);

        //        yield return new WaitForSeconds(0.05f);
        //    }

        //    //不満顔をすぐ解除しないように最低限キープ
        //    yield return new WaitForSeconds(3.0f);

        //    //一定の高さの視点まで来たら解除
        //    while (dir.y < -0.2f)
        //    {
        //        //キープが解除されてたら強制終了
        //        if (!charaCon.keepRunAnime) break;
        //        dir = lookAtCon.virtualHead.InverseTransformPoint(lookAtCon.lookTarget.position).normalized;
        //        yield return new WaitForSeconds(0.1f);
        //    }

        //    //顔解除
        //    for (int i = 0; i <= 5; i++)
        //    {
        //        weight = 50 - (10 * i);
        //        //EYE_DEF_C
        //        //charaCon.facialSync.uniSkin[0].SetBlendShapeWeight(listFaceID[0], weight);
        //        ////EL_DEF_C
        //        //charaCon.facialSync.uniSkin[1].SetBlendShapeWeight(listFaceTransID[0], weight);

        //        weight = 100 - (20 * i);
        //        //random
        //        //charaCon.facialSync.uniSkin[0].SetBlendShapeWeight(faceID, weight);
        //        //charaCon.facialSync.uniSkin[1].SetBlendShapeWeight(faceTransID, weight);

        //        yield return new WaitForSeconds(0.02f);
        //    }

        //    //リップシンクを戻す
        //    //charaCon._lipSync.enabled = true;
        //    isShockSound = false;
        //    faceChanging = false;
        //}

        ///// <summary>
        ///// 表情遷移(VRM用)
        ///// </summary>
        ///// <returns></returns>
        //private IEnumerator ChangeFace_VRM()
        //{
        //    //Conf
        //    VRM.BlendShapePreset preset = VRM.BlendShapePreset.Sorrow;
        //    isAngFace = false;
        //    if (Random.Range(0, 2) == 1)
        //    {
        //        //Ang
        //        isAngFace = true;
        //        preset = VRM.BlendShapePreset.Angry;
        //    }
        //    faceChanging = true;

        //    //競合するのでリップシンクを止める
        //    //charaCon._lipSync.enabled = false;
        //    yield return null;

        //    //表情変更をするのでマニュアルmodeにする
        //    //charaCon.facialSync.isManualControl = true;

        //    //既存表情を全て初期化しておく
        //    //charaCon.facialSync.AllClear_BlendShape();
        //    //charaCon.lipSync.AllClear_BlendShape();

        //    float weight = 0;
        //    //顔遷移
        //    for (int i = 0; i <= 10; i++)
        //    {
        //        weight = 0.1f * i;
        //        //random
        //        //charaCon.facialSync.SetBlendShape(preset, weight);
        //        yield return new WaitForSeconds(0.05f);
        //    }

        //    //不満顔をすぐ解除しないように最低限キープ
        //    yield return new WaitForSeconds(3.0f);

        //    //一定の高さの視点まで来たら解除
        //    while (dir.y < -0.2f)
        //    {
        //        //キープが解除されてたら強制終了
        //        if (!charaCon.keepRunAnime) break;
        //        dir = lookAtCon.virtualHead.InverseTransformPoint(lookAtCon.lookTarget.position).normalized;
        //        yield return new WaitForSeconds(0.1f);
        //    }

        //    //顔解除
        //    for (int i = 0; i <= 5; i++)
        //    {
        //        weight = 1 - (0.2f * i);
        //        //random
        //        //charaCon.facialSync.SetBlendShape(preset, weight);

        //        yield return new WaitForSeconds(0.02f);
        //    }

        //    //リップシンクを戻す
        //    //charaCon._lipSync.enabled = true;
        //    isShockSound = false;
        //    faceChanging = false;

        //    //表情変更モードを戻す
        //    //charaCon.facialSync.isManualControl = false;
        //}
    }

}