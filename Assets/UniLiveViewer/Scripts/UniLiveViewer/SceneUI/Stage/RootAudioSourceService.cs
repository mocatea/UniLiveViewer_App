using System.Linq;
using UniLiveViewer.SceneLoader;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer
{
    /// <summary>
    /// AudioSourceの統括
    /// ミキサー検証済んだら置き換えるかも
    /// </summary>
    public class RootAudioSourceService : MonoBehaviour
    {
        [SerializeField] AudioSource _bgmAudioSource;
        [SerializeField] AudioSource[] _seAudioSources;
        [SerializeField] AudioSource _ambientAudioSources;

        public AudioSource BgmAudioSource => _bgmAudioSource;

        /// <summary> 0~1.0f </summary>
        public float MasterVolumeRate { get; private set; }
        /// <summary> 0~1.0f </summary>
        public IReadOnlyReactiveProperty<float> BGMVolumeRate => _bgmVolumeRate;
        readonly ReactiveProperty<float> _bgmVolumeRate = new();
        float _preBGMVolumeRate;

        /// <summary> 0~1.0f </summary>
        public IReadOnlyReactiveProperty<float> SEVolumeRate => _seVolumeRate;
        readonly ReactiveProperty<float> _seVolumeRate = new();
        float _preSEVolumeRate;

        /// <summary> 0~1.0f </summary>
        public IReadOnlyReactiveProperty<float> AmbientVolumeRate => _ambientVolumeRate;
        readonly ReactiveProperty<float> _ambientVolumeRate = new();
        float _preAmbientVolumeRate;
        /// <summary> 0~1.0f </summary>
        public IReadOnlyReactiveProperty<float> FootStepsVolumeRate => _footStepsVolumeRate;
        readonly ReactiveProperty<float> _footStepsVolumeRate = new();
        float _preFootStepsVolumeRate;

        int _currentSE = 0;
        AudioClipSettings _audioClipSettings;

        [Inject]
        public void Construct(AudioClipSettings audioClipSettings)
        {
            _audioClipSettings = audioClipSettings;
        }

        void Awake()
        {
            MasterVolumeRate = FileReadAndWriteUtility.UserProfile.SoundMaster * 0.01f;
            _preBGMVolumeRate = FileReadAndWriteUtility.UserProfile.SoundBGM * 0.01f;
            _preSEVolumeRate = FileReadAndWriteUtility.UserProfile.SoundSE * 0.01f;
            _preAmbientVolumeRate = FileReadAndWriteUtility.UserProfile.SoundAmbient * 0.01f;
            _preFootStepsVolumeRate = FileReadAndWriteUtility.UserProfile.SoundFootSteps * 0.01f;

            _bgmVolumeRate.Subscribe(x => _bgmAudioSource.volume = x).AddTo(this);
            _seVolumeRate.Subscribe(x =>
                {
                    foreach (var audioSource in _seAudioSources)
                    {
                        audioSource.volume = x;
                    }
                }).AddTo(this);
            _ambientVolumeRate.Subscribe(x => _ambientAudioSources.volume = x).AddTo(this);

            _bgmVolumeRate.Value = _preBGMVolumeRate * MasterVolumeRate;
            _seVolumeRate.Value = _preSEVolumeRate * MasterVolumeRate;
            _ambientVolumeRate.Value = _preAmbientVolumeRate * MasterVolumeRate;
            _footStepsVolumeRate.Value = _preFootStepsVolumeRate;
        }

        public void Start()
        {
            PlayOneShotAmbientAudio();
        }

        /// <param name="volume">0~100</param>
        public void SetMasterVolume(float volume)
        {
            MasterVolumeRate = volume * 0.01f;
            FileReadAndWriteUtility.UserProfile.SoundMaster = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            _bgmVolumeRate.Value = _preBGMVolumeRate * MasterVolumeRate;
            _seVolumeRate.Value = _preSEVolumeRate * MasterVolumeRate;
            _ambientVolumeRate.Value = _preAmbientVolumeRate * MasterVolumeRate;
            _footStepsVolumeRate.Value = _preFootStepsVolumeRate * MasterVolumeRate;
        }

        /// <param name="volume">0~100</param>
        public void SetBGMVolume(float volume)
        {
            _preBGMVolumeRate = volume * 0.01f;
            _bgmVolumeRate.Value = _preBGMVolumeRate * MasterVolumeRate;
            FileReadAndWriteUtility.UserProfile.SoundBGM = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <param name="volume">0~100</param>
        public void SetSEVolume(float volume)
        {
            _preSEVolumeRate = volume * 0.01f;
            _seVolumeRate.Value = _preSEVolumeRate * MasterVolumeRate;
            FileReadAndWriteUtility.UserProfile.SoundSE = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <param name="volume">0~100</param>
        public void SetAmbientVolume(float volume)
        {
            _preAmbientVolumeRate = volume * 0.01f;
            _ambientVolumeRate.Value = _preAmbientVolumeRate * MasterVolumeRate;
            FileReadAndWriteUtility.UserProfile.SoundAmbient = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <param name="volume">0~100</param>
        public void SetFootStepsVolume(float volume)
        {
            _preFootStepsVolumeRate = volume * 0.01f;
            _footStepsVolumeRate.Value = _preFootStepsVolumeRate * MasterVolumeRate;
            FileReadAndWriteUtility.UserProfile.SoundFootSteps = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void PlayOneShot(AudioSE audioType)
        {
            var clip = _audioClipSettings.AudioSEDataSet.FirstOrDefault(x => x.AudioType == audioType).AudioClip;
            GetCurrentAudioSource().PlayOneShot(clip);
        }

        public void PlayOneShotAmbientAudio()
        {
            var clip = _audioClipSettings.GetSceneAudioDataSet(SceneChangeService.GetSceneType).AmbientSoundAudioClip;
            _ambientAudioSources.clip = clip;
            _ambientAudioSources.loop = true;
            _ambientAudioSources.Play();
        }

        AudioSource GetCurrentAudioSource()
        {
            _currentSE++;
            if (_currentSE <= _seAudioSources.Length) _currentSE = 0;
            return _seAudioSources[_currentSE];
        }
    }
}