﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityVMDReader;

namespace UniLiveViewer
{
    public class GeneratorPortal : MonoBehaviour
    {
        //キャラ
        [SerializeField] private List<CharaController> listChara = new List<CharaController>();
        public int currentChara { get; private set; } = 0;

        public int currentAnime { get; private set; } = 0;
        public int currentVMDLipSync { get; private set; } = 0;
        public bool isAnimationReverse = false;
        private const string LIPSYNC_NONAME = "No-LipSyncData";
        private const string LIPSYNC_VIEWNAME = "+ LipSyncData";

        //基本ダンスクリップに上書きするクリップ(手・顔・口パク）
        private Dictionary<string, int> dicAniType = new Dictionary<string, int>() { { "HAND_L", 0 }, { "HAND_R", 1 }, { "FACE", 2 }, { "LIP", 3 } };

        [SerializeField] private DanceInfoData[] danceAniClipInfo;
        [SerializeField] private string[] vmdLipSync;
        public DanceInfoData[] GetDanceInfoData() { return danceAniClipInfo; }
        public string[] GetVmdLipSync() { return vmdLipSync; }
        [SerializeField] private DanceInfoData DanceInfoData_VMDPrefab;//VMD用のテンプレ
        private DanceInfoData[] vmdDanceClipInfo;

        //汎用
        private TimelineController timeline = null;
        private FileAccessManager fileManager = null;

        private VMDPlayer_Custom vmdPlayer;
        private bool isGenerateComplete = true;
        private bool retryVMD = false;
        private CancellationTokenSource cts;

        //読み込み済みVMD情報
        private static Dictionary<string, VMD> dic_VMDReader = new Dictionary<string, VMD>();

        void Awake()
        {
            if (!timeline) timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            fileManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<FileAccessManager>();

            //キャラリストに空枠を追加(空をVRM読み込み枠として扱う、雑仕様)
            listChara.Add(null);
        }

        private void Start()
        {
            //VMD枠はダミーアニメーションを追加しておく
            if (fileManager.vmdList.Count > 0)
            {
                vmdDanceClipInfo = new DanceInfoData[fileManager.vmdList.Count];

                for (int i = 0; i < vmdDanceClipInfo.Length; i++)
                {
                    vmdDanceClipInfo[i] = Instantiate(DanceInfoData_VMDPrefab);

                    vmdDanceClipInfo[i].motionOffsetTime = 0;
                    vmdDanceClipInfo[i].strBeforeName = fileManager.vmdList[i];
                    vmdDanceClipInfo[i].viewName = fileManager.vmdList[i];
                }
                danceAniClipInfo = danceAniClipInfo.Concat(vmdDanceClipInfo).ToArray();
            }
            //口パクVMD
            if (fileManager.vmdLipSyncList.Count > 0)
            {
                string[] lipSyncs = new string[fileManager.vmdLipSyncList.Count];
                lipSyncs = fileManager.vmdLipSyncList.ToArray();

                string[] dummy = { LIPSYNC_NONAME };
                vmdLipSync = dummy.Concat(lipSyncs).ToArray();
            }
        }

        /// <summary>
        /// キャラリストにVRMを追加する
        /// </summary>
        public void AddVRMPrefab(CharaController charaCon_vrm)
        {
            //VRMを追加
            listChara[listChara.Count - 1] = charaCon_vrm;
            //無効化して管理
            charaCon_vrm.gameObject.SetActive(false);
            //リストに空枠を追加(空をVRM読み込み枠として扱う)
            listChara.Add(null);
        }

        /// <summary>
        /// カレントのVRMPrefabを削除する
        /// </summary>
        public void DeleteCurrenVRM()
        {
            if (listChara[currentChara].charaInfoData.formatType
                != CharaInfoData.FORMATTYPE.VRM) return;

            string viewName = listChara[currentChara].charaInfoData.viewName;

            //Prefabから削除
            Destroy(listChara[currentChara].gameObject);
            listChara.RemoveAt(currentChara);

            currentChara--;

            //フィールド上に存在すれば削除
            timeline.DeletebindAsset_CleanUp(viewName);

            //未使用アセットサーチが走り、不要なものを削除
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 指定Currentのキャラをセットする
        /// </summary>
        /// <param Currentを動かす="moveCurrent">動かす必要がなければ0</param>
        public void SetChara(int moveCurrent)
        {
            isGenerateComplete = false;

            currentChara += moveCurrent;

            //Current移動制限
            if (currentChara < 0) currentChara = listChara.Count - 1;
            else if (currentChara >= listChara.Count) currentChara = 0;

            if (!timeline) timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();

            //既存のポータルキャラを削除
            timeline.DestoryPortalChara();

            //フィールド上限オーバーなら生成しない
            if (timeline.FieldCharaCount >= timeline.maxFieldChara) return;

            //null枠の場合も処理しない
            if (!listChara[currentChara]) return;

            //キャラを生成
            var charaCon = Instantiate(listChara[currentChara]);

            bool isSuccess;

            //VRM
            if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                if (!charaCon.gameObject.activeSelf) charaCon.gameObject.SetActive(true);
                
                //パラメーター設定
                charaCon.SetState(CharaController.CHARASTATE.MINIATURE, transform);//ミニチュア状態

                //Timelineのポータル枠へバインドする
                isSuccess = timeline.NewAssetBinding_Portal(charaCon);

                //削除と揺れ物の再稼働
                foreach (var e in charaCon.springBoneList)
                {
                    e.enabled = true;
                }
            }
            else
            {
                //パラメーター設定
                charaCon.SetState(CharaController.CHARASTATE.MINIATURE, transform);//ミニチュア状態

                //Timelineのポータル枠へバインドする
                isSuccess = timeline.NewAssetBinding_Portal(charaCon);
            }

            if (isSuccess) SetAnimation(0).Forget();//キャラにアニメーション情報をセットする
            else if (!isSuccess && charaCon) Destroy(charaCon.gameObject);

            isGenerateComplete = true;
        }

        /// <summary>
        /// 指定Currentのアニメーションをセットする
        /// </summary>
        /// <param Currentを動かす="moveCurrent">動かす必要がなければ0</param>
        public async UniTask SetAnimation(int moveCurrent)
        {
            try
            {
                cts = new CancellationTokenSource();

                //Current移動制限
                currentAnime += moveCurrent;
                if (currentAnime < 0) currentAnime = danceAniClipInfo.Length - 1;
                else if (currentAnime >= danceAniClipInfo.Length) currentAnime = 0;

                //ポータルキャラを確認
                var portalChara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
                if (!portalChara) return;
                vmdPlayer = portalChara.GetComponent<VMDPlayer_Custom>();

                //VMD
                if (GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.VMD)
                {
                    //ポータル上のキャラにアニメーション設定
                    timeline.SetAnimationClip(timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime], transform.position, Vector3.zero);
                    await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);

                    //animatorを停止、VMDを再生
                    string folderPath = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.MOTION);//VMDのパスを取得
                    portalChara.GetComponent<Animator>().enabled = false;//Animatorが競合するので無効  
                    portalChara.animationMode = CharaController.ANIMATIONMODE.VMD;
                    await VMDPlay(vmdPlayer, folderPath, GetNowAnimeInfo().viewName, cts.Token);
                }
                //プリセットアニメーション 
                else
                {
                    //反転設定
                    danceAniClipInfo[currentAnime].isReverse = isAnimationReverse;

                    //VMDを停止、animator再開
                    vmdPlayer.Clear();
                    portalChara.GetComponent<Animator>().enabled = true;
                    portalChara.animationMode = CharaController.ANIMATIONMODE.CLIP;

                    //ポータル上のキャラにアニメーション設定
                    timeline.SetAnimationClip(timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime], transform.position, Vector3.zero);
                    await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                retryVMD = true;
                throw;
            }
        }

        /// <summary>
        /// VMDを再生する
        /// </summary>
        /// <param name="vmpPlayer"></param>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        public async UniTask VMDPlay(VMDPlayer_Custom vmdPlayer, string folderPath, string fileName, CancellationToken token)
        {
            //既存の読み込み済みリストと照合
            if (dic_VMDReader.ContainsKey(fileName))
            {
                //使いまわしてVMDプレイヤースタート
                await vmdPlayer.Starter(dic_VMDReader[fileName], folderPath, fileName, token);
            }
            else
            {
                //新規なら読み込んでVMDプレイヤースタート
                var newVMD = await vmdPlayer.Starter(null, folderPath, fileName, token);
                //新規VMDを登録
                dic_VMDReader.Add(fileName, newVMD);
            }
        }

        /// <summary>
        /// キャラ情報を全取得
        /// </summary>
        /// <returns></returns>
        public CharaInfoData[] GetCharasInfo()
        {
            CharaInfoData[] result = new CharaInfoData[listChara.Count];

            for (int i = 0; i < listChara.Count; i++)
            {
                if (listChara[i]) result[i] = listChara[i].charaInfoData;
                else result[i] = null;
            }
            return result;
        }

        /// <summary>
        /// 現在のキャラ名を取得
        /// </summary>
        /// <returns></returns>
        public bool GetNowCharaName(out string name)
        {
            if (listChara[currentChara])
            {
                name = listChara[currentChara].charaInfoData.viewName;
                return true;
            }
            else
            {
                name = "None";
                return false;
            }
        }

        /// <summary>
        /// 現在のアニメーションクリップ名を取得
        /// </summary>
        /// <returns></returns>
        public DanceInfoData GetNowAnimeInfo()
        {
            return danceAniClipInfo[currentAnime];
        }

        /// <summary>
        /// 現在のリップシンク名を取得
        /// </summary>
        /// <returns></returns>
        public string GetNowLipSyncName()
        {
            string result = vmdLipSync[currentVMDLipSync];
            if (result == LIPSYNC_NONAME) result = LIPSYNC_VIEWNAME;
            return vmdLipSync[currentVMDLipSync];
        }

        private void OnDisable()
        {
            cts.Cancel();
        }

        private void OnEnable()
        {
            //リトライ処理
            if (!isGenerateComplete)
            {
                isGenerateComplete = true;
                SetChara(0);
            }
            else if (retryVMD)
            {
                retryVMD = false;
                SetAnimation(0).Forget();
            }
        }

    }
}