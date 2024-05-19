﻿using Cysharp.Threading.Tasks;
using NanaCiel;
using System.Threading;
using UniLiveViewer.Player;
using UniLiveViewer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using UniRx;

namespace UniLiveViewer.Menu
{
    public class AudioPlaybackPage : MonoBehaviour
    {
        [SerializeField] MenuManager _menuManager;
        [SerializeField] Button_Base[] btn_jumpList;
        [SerializeField] Button_Switch[] _switchAudio = new Button_Switch[2];
        bool _isPresetAudio;

        [SerializeField] Button_Base[] btn_Audio = new Button_Base[2];
        [SerializeField] Button_Base btnS_Play = null;
        [SerializeField] Button_Base btnS_Stop = null;
        [SerializeField] Button_Base btnS_BaseReturn = null;
        [SerializeField] TextMesh[] textMeshs = new TextMesh[4];
        [SerializeField] SliderGrabController slider_Playback = null;
        [SerializeField] SliderGrabController slider_Speed = null;
        PlayableMusicService _playableMusicService;
        PlayableDirector _playableDirector;
        PlayerHandsService _playerHandsService;
        AudioAssetManager _audioAssetManager;
        AudioSourceService _audioSourceService;

        CancellationToken _cancellationToken;

        [Inject]
        public void Construct(
            AudioAssetManager audioAssetManager,
            PlayableMusicService playableMusicService,
            PlayableDirector playableDirector,
            PlayerHandsService playerHandsService,
            AudioSourceService audioSourceService)
        {
            _audioAssetManager = audioAssetManager;
            _playableMusicService = playableMusicService;
            _playableDirector = playableDirector;
            _playerHandsService = playerHandsService;
            _audioSourceService = audioSourceService;
        }

        public void OnJumpSelect((JumpList.TARGET, int) select)
        {
            var target = select.Item1;
            var index = select.Item2;

            int moveIndex = 0;
            switch (target)
            {
                case JumpList.TARGET.AUDIO:
                    if (_isPresetAudio)
                    {
                        moveIndex = index - _audioAssetManager.CurrentPreset;
                    }
                    else
                    {
                        moveIndex = index - _audioAssetManager.CurrentCustom;
                    }
                    ChangeAudioAsync(moveIndex, _cancellationToken).Forget();
                    break;
            }
            _audioSourceService.PlayOneShot(0);
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _isPresetAudio = true;
            _cancellationToken = cancellation;

            //再生スライダーに最大値を設定
            slider_Playback.maxValuel = (float)_playableDirector.duration;

            //ジャンプリスト
            foreach (var e in btn_jumpList)
            {
                e.onTrigger += OpenJumplist;
            }

            _playableDirector.played += OnPlayedDirector;
            _playableDirector.stopped += OnStopedDirector;
            for (int i = 0; i < btn_Audio.Length; i++)
            {
                btn_Audio[i].onTrigger += OnClickMoveIndex;
            }

            slider_Playback.BeginDriveAsObservable
                .Subscribe(_ => OnUpdatePlaybackSlider()).AddTo(this);
            slider_Playback.ValueAsObservable
                .DistinctUntilChanged()
                .Subscribe(value => 
                {
                    _playableMusicService.AudioClipPlaybackTime = value;
                    var sec = _playableMusicService.AudioClipPlaybackTime;
                    textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
                }).AddTo(this);

            slider_Speed.ValueAsObservable
                .Subscribe(value =>
                {
                    _playableMusicService.TimelineSpeed = value;
                    textMeshs[3].text = $"{value:0.00}";
                }).AddTo(this);
            slider_Speed.Value = 1.0f;

            btnS_Play.onTrigger += OnClickPlay;
            btnS_Stop.onTrigger += OnClickStop;
            btnS_BaseReturn.onTrigger += OnClickBaseReturn;
            for (int i = 0; i < _switchAudio.Length; i++)
            {
                _switchAudio[i].isEnable = (i == 0);
                _switchAudio[i].onTrigger += OnClickCategory;
            }

            Initialize();

            //最初のアクターが生成されるのを待つ
            await UniTask.Delay(2000, cancellationToken: cancellation);
            BaseReturnAsync(cancellation).Forget();


            void OnClickPlay(Button_Base btn)
            {
                _audioSourceService.PlayOneShot(0);
                if (_playerHandsService.IsGrabbingSliderWithHands()) return;

                PlayAsync(_cancellationToken).Forget();
            }

            void OnClickStop(Button_Base btn)
            {
                _audioSourceService.PlayOneShot(0);
                if (_playerHandsService.IsGrabbingSliderWithHands()) return;

                StopAsync(_cancellationToken).Forget();
            }

            void OnClickBaseReturn(Button_Base btn)
            {
                _audioSourceService.PlayOneShot(0);
                if (_playerHandsService.IsGrabbingSliderWithHands()) return;

                BaseReturnAsync(_cancellationToken).Forget();
            }

            void OnClickCategory(Button_Base btn)
            {
                var isPresetAudio = false;
                if (_switchAudio[0] == btn)
                {
                    isPresetAudio = true;
                    _switchAudio[0].isEnable = true;
                    _switchAudio[1].isEnable = false;
                }
                else
                {
                    isPresetAudio = false;
                    _switchAudio[0].isEnable = false;
                    _switchAudio[1].isEnable = true;
                }
                _menuManager.jumpList.Close();
                _audioSourceService.PlayOneShot(0);
                ChangeCategoryAsync(isPresetAudio, 0, _cancellationToken).Forget();
            }

            void OnClickMoveIndex(Button_Base btn)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (btn_Audio[i] != btn) continue;

                    var moveIndex = i == 0 ? -1 : 1;
                    ChangeAudioAsync(moveIndex, _cancellationToken).Forget();
                    _audioSourceService.PlayOneShot(0);
                    return;
                }
            }
        }

        void OnEnable()
        {
            Initialize();
        }

        async void Initialize()
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                btnS_Stop.gameObject.SetActive(false);
                btnS_Play.gameObject.SetActive(true);
            }
            else
            {
                btnS_Stop.gameObject.SetActive(true);
                btnS_Play.gameObject.SetActive(false);
            }
            //オーディオの長さ
            var sec = await _playableMusicService.CurrentAudioLengthAsync(true, _cancellationToken);
            textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
        }

        void Update()
        {
            //再生スライダー非制御中なら
            if (!slider_Playback.IsGrabbed)
            {
                //TimeLine再生時間をスライダーにセット
                var sec = (float)_playableMusicService.AudioClipPlaybackTime;
                slider_Playback.NotNotifyChangeValue(sec);
                textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            }
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        void OpenJumplist(Button_Base btn)
        {
            if (!_menuManager.jumpList.gameObject.activeSelf) _menuManager.jumpList.gameObject.SetActive(true);

            if (btn == btn_jumpList[0])
            {
                _menuManager.jumpList.SetAudioData(_isPresetAudio);
            }
            _audioSourceService.PlayOneShot(0);
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        async UniTask ChangeAudioAsync(int moveIndex, CancellationToken cancellation)
        {
            var clipName = await _playableMusicService.NextAudioClip(_isPresetAudio, moveIndex, cancellation);
            await ChangeAuidoInternalAsync(clipName, cancellation);
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        async UniTask ChangeCategoryAsync(bool isPreset, int moveIndex, CancellationToken cancellation)
        {
            _isPresetAudio = isPreset;
            var clipName = await _playableMusicService.NextAudioClip(isPreset, moveIndex, cancellation);
            await ChangeAuidoInternalAsync(clipName, cancellation);
        }

        async UniTask ChangeAuidoInternalAsync(string clipName, CancellationToken cancellation)
        {
            textMeshs[0].text = clipName;
            textMeshs[0].fontSize = clipName.FontSizeMatch(600, 30, 50);

            if (clipName == string.Empty)
            {
                Debug.LogWarning("No custom songs.");
                return;
            }

            var sec = await _playableMusicService.CurrentAudioLengthAsync(_isPresetAudio, cancellation);
            slider_Playback.maxValuel = sec;
            textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
        }

        void OnPlayedDirector(PlayableDirector obj)
        {
            //停止表示
            //btnS_Stop.gameObject.SetActive(true);
            //btnS_Play.gameObject.SetActive(false);
        }
        void OnStopedDirector(PlayableDirector obj)
        {
            //再生途中の一時停止は無視する
            if (_playableMusicService.AudioClipPlaybackTime > 0) return;

            //再生表示
            if (btnS_Stop) btnS_Stop.gameObject.SetActive(false);
            if (btnS_Play) btnS_Play.gameObject.SetActive(true);
        }

        void OnUpdatePlaybackSlider()
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) return;

            btnS_Stop.gameObject.SetActive(false);
            btnS_Play.gameObject.SetActive(true);

            var dummy = new CancellationToken();
            _playableMusicService.ManualModeAsync(dummy).Forget();
        }

        async UniTask PlayAsync(CancellationToken cancellation)
        {
            btnS_Stop.gameObject.SetActive(true);
            btnS_Play.gameObject.SetActive(false);

            await _playableMusicService.PlayAsync(cancellation);
        }

        async UniTask StopAsync(CancellationToken cancellation)
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) return;

            btnS_Stop.gameObject.SetActive(false);
            btnS_Play.gameObject.SetActive(true);

            await _playableMusicService.ManualModeAsync(cancellation);
        }

        async UniTask BaseReturnAsync(CancellationToken cancellation)
        {
            await _playableMusicService.BaseReturnAsync(cancellation);
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                var dummy = new CancellationToken();
                PlayAsync(dummy).Forget();
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                var dummy = new CancellationToken();
                BaseReturnAsync(dummy).Forget();
            }
            if (Input.GetKeyDown(KeyCode.K)) ChangeAudioAsync(1, _cancellationToken).Forget();
            if (Input.GetKeyDown(KeyCode.J)) ChangeAudioAsync(-1, _cancellationToken).Forget();
        }
    }
}
