using UnityEngine;
using UnityEngine.Localization;

namespace UniLiveViewer.SO
{
    [CreateAssetMenu(menuName = "MyGame/Audio/MusicSetting", fileName = "MusicSetting")]
    public class MusicSetting : ScriptableObject
    {
        public AudioClip Clip => _clip;
        [SerializeField] AudioClip _clip;

        /// <summary> 翻訳で切り替わる </summary>
        public LocalizedString DisplayName => _displayName;
        [SerializeField] LocalizedString _displayName;
    }
}