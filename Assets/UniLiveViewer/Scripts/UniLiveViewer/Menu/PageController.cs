﻿using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer.Menu
{
    public class PageController : MonoBehaviour
    {
        public event Action onSwitchPage;
        [Header("＜ページ・タブボタン＞")]
        [SerializeField] Button_Switch[] btnTab;
        [SerializeField] Transform[] pageAnchor;
        public Button_Switch[] BtnTab => btnTab;
        public Transform[] GetPageAnchor => pageAnchor;

        CancellationToken cancellation_token;
        public int current { get; private set; }
        public Transform GetCurrentPage() { return pageAnchor[current]; } 

        /// <summary>
        /// TODO: Serviceに代替したい
        /// </summary>
        [Header("＜Sound＞")]
        [SerializeField] AudioClip[] Sound;//ボタン音
        AudioSource audioSource;


        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;
            audioSource.enabled = false;

            DelayAudioSource().Forget();
        }

        async UniTask DelayAudioSource()
        {
            await UniTask.Delay(1000);
            audioSource.enabled = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (btnTab.Length != pageAnchor.Length)
            {
                Debug.LogError("ページとボタン数が一致しません");
                return;
            }

            for (int i = 0; i < btnTab.Length; i++)
            {
                btnTab[i].onTrigger += SwitchCurrent;
            }

            cancellation_token = this.GetCancellationTokenOnDestroy();

            SwitchPages();
        }

        /// <summary>
        /// ページを切り替える
        /// </summary>
        /// <param name="btn"></param>
        void SwitchCurrent(Button_Base btn)
        {
            //タブのボタン状態を更新する
            for (int i = 0; i < btnTab.Length; i++)
            {
                if (btnTab[i] != btn) continue;
                current = i;
                break;
            }
            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            SwitchPages();
        }

        /// <summary>
        /// カレントページに切り替える
        /// </summary>
        void SwitchPages()
        {
            //カレントページのタブボタンが非表示の場合別ページへ
            if (!btnTab[current].gameObject.activeSelf)
            {
                for (int i = 0; i < btnTab.Length; i++)
                {
                    if (!btnTab[i]) continue;
                    if (!btnTab[i].gameObject.activeSelf) continue;
                    current = i;
                    break;
                }
            }

            //ページ切り替え
            bool b = false;
            for (int i = 0; i < btnTab.Length; i++)
            {
                if (!btnTab[i]) continue;
                b = current == i;
                btnTab[i].isEnable = b;
                if (pageAnchor[i].gameObject.activeSelf != b) pageAnchor[i].gameObject.SetActive(b);
            }

            onSwitchPage?.Invoke();

            UniTask.Void(async () =>
            {
                await UniTask.Delay(400, cancellationToken: cancellation_token);
                //ボタンの揺れ音
                audioSource.PlayOneShot(Sound[1]);
            });
        }

        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        void OnEnable()
        {
            SwitchPages();
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                current++;
                if (current >= pageAnchor.Length) current = 0;
                audioSource.PlayOneShot(Sound[0]);
                SwitchPages();
            }
        }
    }
}

