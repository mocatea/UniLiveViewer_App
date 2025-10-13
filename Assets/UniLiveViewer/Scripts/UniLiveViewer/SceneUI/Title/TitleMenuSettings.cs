using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UniLiveViewer.SceneUI.Title
{
    public class TitleMenuSettings : MonoBehaviour
    {
        public Transform UiRoot => _uiRoot;
        [SerializeField] Transform _uiRoot;

        public Transform MainMenuCanvas => _mainMenuCanvas;
        [SerializeField] Transform _mainMenuCanvas;

        public List<Button> MainMenuButton => _mainMenuButton;
        [SerializeField] List<Button> _mainMenuButton;

        public Transform CustomLiveCanvas => _customLiveCanvas;
        [SerializeField] Transform _customLiveCanvas;

        public List<Button> CustomLiveButton => _customLiveButton;
        [SerializeField] List<Button> _customLiveButton;

        public Transform LicenseCanvas => _licenseCanvas;
        [SerializeField] Transform _licenseCanvas;

        public List<Button> LicenseButton => _licenseButton;
        [SerializeField] List<Button> _licenseButton;

        public AudioSourceService AudioSourceService => _audioSourceService;
        [SerializeField] AudioSourceService _audioSourceService;

        void Awake()
        {
            Assert.IsNotNull(_uiRoot);
            Assert.IsNotNull(_mainMenuCanvas);
            Assert.IsNotNull(_mainMenuButton);
            Assert.IsNotNull(_customLiveCanvas);
            Assert.IsNotNull(_customLiveButton);
            Assert.IsNotNull(_licenseCanvas);
            Assert.IsNotNull(_licenseButton);
            Assert.IsNotNull(_audioSourceService);

            for (int i = 0; i < _mainMenuButton.Count; i++)
            {
                Assert.IsNotNull(_mainMenuButton[i]);
            }
            for (int i = 0; i < _customLiveButton.Count; i++)
            {
                Assert.IsNotNull(_customLiveButton[i]);
            }
            for (int i = 0; i < _licenseButton.Count; i++)
            {
                Assert.IsNotNull(_licenseButton[i]);
            }
        }
    }
}