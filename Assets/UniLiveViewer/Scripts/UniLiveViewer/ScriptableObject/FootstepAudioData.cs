using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer.SO
{
    [CreateAssetMenu(menuName = "MyGame/Audio/FootstepAudioData", fileName = "Footstep_")]
    [System.Serializable]
    public class FootstepAudioData : ScriptableObject
    {
        public IReadOnlyList<AudioClip> AudioClip => _audioClip;
        [SerializeField] List<AudioClip> _audioClip;
    }
}