using System.Collections.Generic;
using System.Linq;
using UniLiveViewer.SceneLoader;
using UnityEngine;

namespace UniLiveViewer.SO
{
    [CreateAssetMenu(menuName = "MyGame/Audio/AudioClipSettings", fileName = "AudioClipSettings")]
    public class AudioClipSettings : ScriptableObject
    {
        public List<AudioClip> AudioBGM => _presetBGM;
        [SerializeField] List<AudioClip> _presetBGM;

        public List<AudioSEDataSet> AudioSEDataSet => _audioSEDataSet;
        [SerializeField] List<AudioSEDataSet> _audioSEDataSet;

        public List<AudioHandPsylliumDataSet> AudioHandPsylliumDataSet => _audioHandPsylliumDataSet;
        [SerializeField] List<AudioHandPsylliumDataSet> _audioHandPsylliumDataSet;

        public FootstepAudioData FootWaterMoveAudioData => _footWaterMoveAudioData;
        [SerializeField] FootstepAudioData _footWaterMoveAudioData;
        public FootstepAudioData RaisedFootWaterSplashAudioData => _raisedfootWaterSplashAudioData;
        [SerializeField] FootstepAudioData _raisedfootWaterSplashAudioData;
        public FootstepAudioData LoweredFeetWaterSplashAudioData => _loweredFeetWaterSplashAudioData;
        [SerializeField] FootstepAudioData _loweredFeetWaterSplashAudioData;

        public SceneAudioDataSet GetSceneAudioDataSet(SceneType sceneType)
            => _sceneAudioDataSet?.FirstOrDefault(x => x.SceneType == sceneType);
        [SerializeField] SceneAudioDataSet[] _sceneAudioDataSet;
    }

    [System.Serializable]
    public class AudioSEDataSet
    {
        public AudioClip AudioClip => _audioClip;
        [SerializeField] AudioClip _audioClip;
        public AudioSE AudioType => _audioType;
        [SerializeField] AudioSE _audioType;
    }

    [System.Serializable]
    public class AudioHandPsylliumDataSet
    {
        public AudioClip AudioClip => _audioClip;
        [SerializeField] AudioClip _audioClip;
        public AudioHandPsylliumSE AudioType => _audioType;
        [SerializeField] AudioHandPsylliumSE _audioType;
    }

    [System.Serializable]
    public class SceneAudioDataSet
    {
        public SceneType SceneType => _sceneType;
        [SerializeField] SceneType _sceneType;

        public AudioClip AmbientSoundAudioClip => _ambientSoundAudioClip;
        [SerializeField] AudioClip _ambientSoundAudioClip;

        public FootstepAudioData FootstepsAudioData => _footstepsAudioData;
        [SerializeField] FootstepAudioData _footstepsAudioData;
    }
}