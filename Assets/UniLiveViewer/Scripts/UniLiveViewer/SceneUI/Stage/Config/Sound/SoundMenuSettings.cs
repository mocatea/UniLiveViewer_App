using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Sound
{
    public class SoundMenuSettings : MonoBehaviour
    {
        public IReadOnlyList<SliderGrabController> SoundSlider => _soundSlider;
        [SerializeField] List<SliderGrabController> _soundSlider;

        public IReadOnlyList<TextMesh> SoundText => _soundText;
        [SerializeField] List<TextMesh> _soundText;

        void Awake()
        {
            Assert.IsNotNull(_soundSlider);
            Assert.IsNotNull(_soundText);

            for (int i = 0; i < _soundSlider.Count; i++)
            {
                Assert.IsNotNull(_soundSlider[i]);
            }
            for (int i = 0; i < _soundText.Count; i++)
            {
                Assert.IsNotNull(_soundText[i]);
            }
        }
    }
}