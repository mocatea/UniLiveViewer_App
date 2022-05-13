﻿using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniLiveViewer 
{
    public class TitleScene : MonoBehaviour
    {
        public OVRScreenFade fade;
        [SerializeField] private Sprite[] sprPrefab = new Sprite[2];
        [SerializeField] private SpriteRenderer sprRender;
        [SerializeField] private TextMesh text_Version;
        [SerializeField] private Button_Base[] btn_Language = new Button_Base[2];

        [SerializeField] private GameObject manualHand;
        [SerializeField] private Transform[] Chara = new Transform[3];

        [SerializeField] private string nextSceneName = "LiveScene";

        //クリックSE
        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//ボタン音

        private CancellationToken cancellation_Token;
        private void Awake()
        {
            for (int i = 0; i < btn_Language.Length; i++)
            {
                btn_Language[i].onTrigger += onChangeLanguage;
            }

            text_Version.text = "ver." + Application.version;

            manualHand.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);

            cancellation_Token = this.GetCancellationTokenOnDestroy();
        }

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;

            //randomにキャラが変わる
            int r = Random.Range(0, 3);
            for (int i = 0; i < 3; i++)
            {
                if (r == i) Chara[i].gameObject.SetActive(true);
                else Chara[i].gameObject.SetActive(false);
            }

            if (SystemInfo.userProfile.data.LanguageCode != (int)USE_LANGUAGE.NULL)
            {
                //2回目以降
                sprRender.gameObject.SetActive(false);
                SceneChange().Forget();
            }
            else InitHand().Forget();
        }

        private async UniTask InitHand()
        {
            await UniTask.Delay(2000, cancellationToken: cancellation_Token);
            transform.GetChild(0).gameObject.SetActive(true);
            await UniTask.Delay(1000, cancellationToken: cancellation_Token);
            manualHand.SetActive(true);
        }

        private void onChangeLanguage(Button_Base btn)
        {
            if (btn.name.Contains("_JP"))
            {
                SystemInfo.userProfile.data.LanguageCode = (int)USE_LANGUAGE.JP;
                SystemInfo.userProfile.WriteJson();
                //差し替える
                sprRender.sprite = sprPrefab[1];
            }
            else
            {
                SystemInfo.userProfile.data.LanguageCode = (int)USE_LANGUAGE.EN;
                SystemInfo.userProfile.WriteJson();
                //差し替える
                sprRender.sprite = sprPrefab[0];
            }

            //クリック音
            audioSource.PlayOneShot(Sound[0]);
            //Handを消す
            manualHand.SetActive(false);

            SceneChange().Forget();
        }

        private async UniTask SceneChange()
        {
            await UniTask.Delay(500, cancellationToken: cancellation_Token);
            if (transform.GetChild(0).gameObject.activeSelf) transform.GetChild(0).gameObject.SetActive(false);

            await UniTask.Delay(1000, cancellationToken: cancellation_Token);
            fade.FadeOut();

            //ロードが早すぎても最低演出分は待機する
            var async = SceneManager.LoadSceneAsync(nextSceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(2000, cancellationToken: cancellation_Token);
            async.allowSceneActivation = true;
        }
    }

}
