using Cysharp.Threading.Tasks;
using NanaCiel;
using System.Threading;
using System.Threading.Tasks;
using UniLiveViewer.Player;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class AudioPlaybackPage : MonoBehaviour
    {
        [SerializeField] MenuManager _menuManager;
        [SerializeField] Button_Base[] btn_jumpList;
        [SerializeField] Button_Base[] _jumpListButtons;
        [SerializeField] Button_Switch[] _switchAudio = new Button_Switch[2];
        bool _isPresetAudio;

        [SerializeField] Button_Base[] _audioButton = new Button_Base[2];
        [SerializeField] Button_Base _playButton = null;
        [SerializeField] Button_Base _pauseButton = null;
        [SerializeField] Button_Base _stopButton = null;
        [SerializeField] TextMesh[] _textMeshs = new TextMesh[4];
        [SerializeField] SliderGrabController _playbackSlider = null;
        [SerializeField] SliderGrabController _playbackSpeedSlider = null;

        TimelineService _timelineService;
        PlayableDirector _playableDirector;
        PlayerHandsService _playerHandsService;
        AudioAssetManager _audioAssetManager;
        RootAudioSourceService _audioSourceService;
        TimelineAudioClipSwitcherService _timelineAudioClipSwitcher;

        CancellationToken _cancellationToken;

        [Inject]
        public void Construct(
            AudioAssetManager audioAssetManager,
            TimelineService timelineService,
            PlayableDirector playableDirector,
            PlayerHandsService playerHandsService,
            TimelineAudioClipSwitcherService timelineAudioClipSwitcher,
            RootAudioSourceService audioSourceService)
        {
            _audioAssetManager = audioAssetManager;
            _timelineService = timelineService;
            _playableDirector = playableDirector;
            _playerHandsService = playerHandsService;
            _audioSourceService = audioSourceService;
            _timelineAudioClipSwitcher = timelineAudioClipSwitcher;
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
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _isPresetAudio = true;
            _cancellationToken = cancellation;

            //再生スライダーに最大値を設定
            _playbackSlider.maxValuel = (float)_playableDirector.duration;

            //ジャンプリスト
            foreach (var e in btn_jumpList)
            {
                e.onTrigger += OpenJumplist;
            }

            _playableDirector.played += OnPlayedDirector;
            _playableDirector.stopped += OnStopedDirector;
            for (int i = 0; i < _audioButton.Length; i++)
            {
                _audioButton[i].onTrigger += OnClickMoveIndex;
            }

            _playbackSlider.BeginDriveAsObservable
                .Subscribe(_ => OnUpdatePlaybackSlider()).AddTo(this);
            _playbackSlider.ValueAsObservable
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    _timelineService.AudioClipPlaybackTime = value;
                    var sec = _timelineService.AudioClipPlaybackTime;
                    _textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
                }).AddTo(this);

            _playbackSpeedSlider.ValueAsObservable
                .Subscribe(value =>
                {
                    _timelineService.TimelineSpeed = value;
                    _textMeshs[3].text = $"{value:0.00}";
                }).AddTo(this);
            _playbackSpeedSlider.Value = 1.0f;

            _playButton.onTrigger += OnClickPlay;
            _pauseButton.onTrigger += OnClickPause;
            _stopButton.onTrigger += OnClickStop;
            for (int i = 0; i < _switchAudio.Length; i++)
            {
                _switchAudio[i].isEnable = (i == 0);
                _switchAudio[i].onTrigger += OnClickCategory;
            }

            await InitializeAsync(_cancellationToken);

            //最初のアクターが生成されるのを待つ
            await UniTask.Delay(2000, cancellationToken: cancellation);
            StopAsync(cancellation).Forget();


            void OnClickPlay(Button_Base btn)
            {
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                if (_playerHandsService.IsGrabbingSliderWithHands()) return;

                PlayAsync(_cancellationToken).Forget();
            }

            void OnClickPause(Button_Base btn)
            {
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                if (_playerHandsService.IsGrabbingSliderWithHands()) return;

                PauseAsync(_cancellationToken).Forget();
            }

            void OnClickStop(Button_Base btn)
            {
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                if (_playerHandsService.IsGrabbingSliderWithHands()) return;

                StopAsync(_cancellationToken).Forget();
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
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                ChangeCategoryAsync(isPresetAudio, 0, _cancellationToken).Forget();
            }

            void OnClickMoveIndex(Button_Base btn)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (_audioButton[i] != btn) continue;

                    var moveIndex = i == 0 ? -1 : 1;
                    ChangeAudioAsync(moveIndex, _cancellationToken).Forget();
                    _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                    return;
                }
            }
        }

        void OnEnable()
        {
            InitializeAsync(_cancellationToken).Forget();
        }

        async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                _pauseButton.gameObject.SetActive(false);
                _playButton.gameObject.SetActive(true);
            }
            else
            {
                _pauseButton.gameObject.SetActive(true);
                _playButton.gameObject.SetActive(false);
            }

            await UpdateAudioMaxLength(cancellationToken);
        }

        void Update()
        {
            //再生スライダー非制御中なら
            if (!_playbackSlider.IsGrabbed)
            {
                //TimeLine再生時間をスライダーにセット
                var sec = (float)_timelineService.AudioClipPlaybackTime;
                _playbackSlider.SetValueWithoutNotify(sec);
                _textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
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
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        async UniTask ChangeAudioAsync(int moveIndex, CancellationToken cancellation)
        {
            var clipName = await _timelineAudioClipSwitcher.SetAudioClipAsync(_isPresetAudio, moveIndex, cancellation);

            if (string.IsNullOrEmpty(clipName)) clipName = TimelineConstants.NoCustomBGMMessage;
            await ChangeAuidoInternalAsync(clipName, cancellation);
        }

        async UniTask ChangeCategoryAsync(bool isPreset, int moveIndex, CancellationToken cancellation)
        {
            _isPresetAudio = isPreset;
            var clipName = await _timelineAudioClipSwitcher.SetAudioClipAsync(isPreset, moveIndex, cancellation);
            if (string.IsNullOrEmpty(clipName)) clipName = TimelineConstants.NoCustomBGMMessage;
            await ChangeAuidoInternalAsync(clipName, cancellation);
        }

        async UniTask ChangeAuidoInternalAsync(string clipName, CancellationToken cancellation)
        {
            _textMeshs[0].text = clipName;
            _textMeshs[0].fontSize = clipName.FontSizeMatch(600, 30, 50);

            if (clipName == string.Empty)
            {
                Debug.LogWarning("No custom songs.");
                return;
            }

            await UpdateAudioMaxLength(cancellation);
        }

        async UniTask UpdateAudioMaxLength(CancellationToken cancellation)
        {
            var sec = await _timelineAudioClipSwitcher.GetCurrentAudioLengthAsync(_isPresetAudio, cancellation);
            _playbackSlider.maxValuel = sec;
            _textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
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
            if (_timelineService.AudioClipPlaybackTime > 0) return;

            //再生表示
            if (_pauseButton) _pauseButton.gameObject.SetActive(false);
            if (_playButton) _playButton.gameObject.SetActive(true);
        }

        void OnUpdatePlaybackSlider()
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) return;

            _pauseButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(true);

            var dummy = new CancellationToken();
            _timelineService.PauseAsync(dummy).Forget();
        }

        async UniTask PlayAsync(CancellationToken cancellation)
        {
            _pauseButton.gameObject.SetActive(true);
            _playButton.gameObject.SetActive(false);

            await _timelineService.PlayAsync(cancellation);
        }

        async UniTask PauseAsync(CancellationToken cancellation)
        {
            _pauseButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(true);

            await _timelineService.PauseAsync(cancellation);
        }

        async UniTask StopAsync(CancellationToken cancellation)
        {
            await _timelineService.StopAsync(cancellation);
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
                StopAsync(dummy).Forget();
            }
            if (Input.GetKeyDown(KeyCode.K)) ChangeAudioAsync(1, _cancellationToken).Forget();
            if (Input.GetKeyDown(KeyCode.J)) ChangeAudioAsync(-1, _cancellationToken).Forget();
        }
    }
}
