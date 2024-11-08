﻿using UnityEngine;

namespace UniLiveViewer.Menu.Config.Common
{
    public class CommonMenuSettings : MonoBehaviour
    {
        public Button_Base VibrationButton => _vibrationButton;
        [SerializeField] Button_Base _vibrationButton;

        public Button_Base PassthroughButton => _passthroughButton;
        [SerializeField] Button_Base _passthroughButton;

        public SliderGrabController FixedFoveatedSlider => _fixedFoveatedSlider;
        [SerializeField] SliderGrabController _fixedFoveatedSlider;

        public TextMesh FixedFoveatedText => _fixedFoveatedText;
        [SerializeField] TextMesh _fixedFoveatedText;

        public Button_Base EnglishButton => _englishButton;
        [SerializeField] Button_Base _englishButton;

        public Button_Base JapaneseButton => _japaneseButton;
        [SerializeField] Button_Base _japaneseButton;
    }
}
