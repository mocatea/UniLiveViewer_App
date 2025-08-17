using Cysharp.Threading.Tasks;
using NanaCiel;
using System;
using System.Threading;
using UniLiveViewer.Player;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;

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
        [SerializeField] Button_Base _loopButton = null;
        [SerializeField] Button_Base _nonLoopButton = null;
        [SerializeField] TextMesh[] _textMeshs = new TextMesh[4];
        [SerializeField] SliderGrabController _playbackSlider = null;
        [SerializeField] SliderGrabController _playbackSpeedSlider = null;
        public IReactiveProperty<float> AudioLength => _audioLength;
        readonly ReactiveProperty<float> _audioLength = new(0);

        public IObservable<Unit> PlayAsObservable => _playStream;
        readonly Subject<Unit> _playStream = new();
        public IObservable<Unit> PauseAsObservable => _pauseStream;
        readonly Subject<Unit> _pauseStream = new();

        public IObservable<Unit> StopAsObservable => _stopStream;
        readonly Subject<Unit> _stopStream = new();

        public IReactiveProperty<float> PlaybackTime => _playbackTime;
        readonly ReactiveProperty<float> _playbackTime = new(0);
        public IReactiveProperty<float> Speed => _speed;
        readonly ReactiveProperty<float> _speed = new(1);

        public IReactiveProperty<bool> IsLoop => _isLoop;
        readonly ReactiveProperty<bool> _isLoop = new(true);

        PlayableDirector _playableDirector;
        PlayerHandsService _playerHandsService;
        TimelineAudioClipSwitcherService _timelineAudioClipSwitcher;
        RootAudioSourceService _audioSourceService;

        CancellationToken _cancellationToken;

        public void Initialize(
            AudioAssetManager audioAssetManager,
            PlayableDirector playableDirector,
            PlayerHandsService playerHandsService,
            TimelineAudioClipSwitcherService timelineAudioClipSwitcher,
            RootAudioSourceService audioSourceService)
        {
            _playableDirector = playableDirector;
            _playerHandsService = playerHandsService;
            _timelineAudioClipSwitcher = timelineAudioClipSwitcher;
            _audioSourceService = audioSourceService;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _isPresetAudio = true;
            _cancellationToken = cancellation;

            //再生スライダーに最大値を設定
            _playbackSlider.SetMaxValuel((float)_playableDirector.duration);

            //ジャンプリスト
            foreach (var e in btn_jumpList)
            {
                e.onTrigger += OpenJumplist;
            }

            for (int i = 0; i < _audioButton.Length; i++)
            {
                _audioButton[i].onTrigger += OnClickMoveIndex;
            }

            _playbackSlider.BeginDriveAsObservable
                .Subscribe(_ => OnUpdatePlaybackSlider()).AddTo(this);
            _playbackSlider.ValueAsObservable
                .DistinctUntilChanged()
                .Subscribe(sec =>
                {
                    _playbackTime.Value = sec;
                    _textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
                }).AddTo(this);

            _playbackSpeedSlider.ValueAsObservable
                .Subscribe(value =>
                {
                    _speed.Value = value;
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
            _loopButton.onTrigger += OnClickLoop;
            _nonLoopButton.onTrigger += OnClickNonLoop;

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

            void OnClickLoop(Button_Base btn)
            {
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                // 反転
                _loopButton.gameObject.SetActive(false);
                _nonLoopButton.gameObject.SetActive(true);
                _isLoop.Value = false;
            }

            void OnClickNonLoop(Button_Base btn)
            {
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                // 反転
                _loopButton.gameObject.SetActive(true);
                _nonLoopButton.gameObject.SetActive(false);
                _isLoop.Value = true;
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

            if (_isLoop.Value)
            {
                _loopButton.gameObject.SetActive(true);
                _nonLoopButton.gameObject.SetActive(false);
            }
            else
            {
                _loopButton.gameObject.SetActive(false);
                _nonLoopButton.gameObject.SetActive(true);
            }

            await UpdateAudioMaxLength(cancellationToken);
        }

        public void OnTick(float audioClipPlaybackTime)
        {
            //再生スライダー非制御中なら
            if (!_playbackSlider.IsGrabbed)
            {
                //TimeLine再生時間をスライダーにセット
                var sec = audioClipPlaybackTime;
                _playbackSlider.SetValueWithoutNotify(sec);
                _textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            }

#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        public void OnChangeTimelineUpdateMode(DirectorUpdateMode mode)
        {
            if (mode == DirectorUpdateMode.Manual)
            {
                _pauseButton.gameObject.SetActive(false);
                _playButton.gameObject.SetActive(true);
            }
            else
            {
                _pauseButton.gameObject.SetActive(true);
                _playButton.gameObject.SetActive(false);
            }
        }

        public void OnJumpSelect((JumpList.TARGET target, int index) select, (int presetIndex, int customIndex) current)
        {
            if (select.target != JumpList.TARGET.AUDIO) return;

            int moveIndex = 0;
            if (_isPresetAudio)
            {
                moveIndex = select.index - current.presetIndex;
            }
            else
            {
                moveIndex = select.index - current.customIndex;
            }
            ChangeAudioAsync(moveIndex, _cancellationToken).Forget();
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void OpenJumplist(Button_Base btn)
        {
            if (!_menuManager.jumpList.gameObject.activeSelf) _menuManager.jumpList.gameObject.SetActive(true);

            if (btn == btn_jumpList[0])
            {
                _menuManager.jumpList.SetAudioAsync(_isPresetAudio).Forget();
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
            _textMeshs[0].SetAutoSizedText(clipName, 0.45f, 40);

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
            _playbackSlider.SetMaxValuel(sec);
            _textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            _audioLength.Value = sec;
        }

        void OnUpdatePlaybackSlider()
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) return;
            _pauseStream.OnNext(Unit.Default);
        }

        async UniTask PlayAsync(CancellationToken cancellation)
        {
            _playStream.OnNext(Unit.Default);
            await UniTask.CompletedTask;
        }

        async UniTask PauseAsync(CancellationToken cancellation)
        {
            _pauseStream.OnNext(Unit.Default);
            await UniTask.CompletedTask;
        }

        async UniTask StopAsync(CancellationToken cancellation)
        {
            _stopStream.OnNext(Unit.Default);
            await UniTask.CompletedTask;
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                var dummy = new CancellationToken();
                if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
                {
                    PlayAsync(dummy).Forget();
                }
                else
                {
                    PauseAsync(dummy).Forget();
                }
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
