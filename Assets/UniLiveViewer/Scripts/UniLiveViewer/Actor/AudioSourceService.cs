using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Actor
{
    /// <summary>
    /// 足音用
    /// </summary>
    public class AudioSourceService : MonoBehaviour
    {
        [SerializeField] AudioSource[] _audioSources;
        int _current = 0;

        void Awake()
        {
            Assert.IsNotNull(_audioSources);
            foreach (var audioSource in _audioSources)
            {
                Assert.IsNotNull(audioSource);
                audioSource.volume = 1;
            }
        }

        public void SetVolume(float volume)
        {
            foreach (var audioSource in _audioSources)
            {
                audioSource.volume = volume;
            }
        }

        public void PlayOneShot(AudioClip audioClip,Vector3 pos)
        {
            _audioSources[_current].transform.position = pos;
            _audioSources[_current].PlayOneShot(audioClip);
            _current++;
            if (_audioSources.Length <= _current) _current = 0;
        }
    }
}