﻿using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class TitleMenuService : MonoBehaviour
    {
        public IObservable<string> ChangeSceneAsObservable => _changeSceneStream;
        Subject<string> _changeSceneStream;

        [SerializeField] Transform _uiRoot;
        [SerializeField] Button_Base[] _languageButton = new Button_Base[2];
        [SerializeField] AudioClip[] _audioClips;
        
        AudioSource _audioSource;
        CancellationToken _cancellationToken;

        SceneChangeService _sceneChangeService;

        void Awake()
        {
            _cancellationToken = this.GetCancellationTokenOnDestroy();
            _changeSceneStream = new Subject<string>();

            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = SystemInfo.soundVolume_SE;
        }

        [Inject]
        void Construct(SceneChangeService sceneChangeService)
        {
            _sceneChangeService = sceneChangeService;
        }

        void Start()
        {
            for (int i = 0; i < _languageButton.Length; i++)
            {
                _languageButton[i].onTrigger += (btn) => OnChangeLanguage(btn, _cancellationToken).Forget();
            }

            if (FileReadAndWriteUtility.UserProfile.LanguageCode == (int)USE_LANGUAGE.NULL)
            {
                //初回
                _uiRoot.gameObject.SetActive(true);
            }
            else
            {
                _uiRoot.gameObject.SetActive(false);
                aa(_cancellationToken).Forget();
            }
        }

        async UniTask aa(CancellationToken cancellationToken)
        {
            var name = FileReadAndWriteUtility.UserProfile.LastSceneName;
            await _sceneChangeService.Change(name, cancellationToken);
        }

        async UniTask OnChangeLanguage(Button_Base btn, CancellationToken cancellationToken)
        {
            var code = btn.name.Contains("_JP") ? USE_LANGUAGE.JP : USE_LANGUAGE.EN;
            FileReadAndWriteUtility.UserProfile.LanguageCode = (int)code;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            //クリック音
            _audioSource.PlayOneShot(_audioClips[0]);
            _changeSceneStream.OnNext(btn.name);

            await UniTask.Delay(500, cancellationToken: cancellationToken);
            if (_uiRoot.gameObject.activeSelf) _uiRoot.gameObject.SetActive(false);

            var name = FileReadAndWriteUtility.UserProfile.LastSceneName;
            await _sceneChangeService.Change(name, cancellationToken);
        }
    }
}