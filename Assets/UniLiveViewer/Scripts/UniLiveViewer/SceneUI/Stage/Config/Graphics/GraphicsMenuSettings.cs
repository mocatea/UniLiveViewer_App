using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuSettings : MonoBehaviour
    {
        public IReadOnlyList<Button_Base> GraphicButton => _graphicButton;
        [SerializeField] List<Button_Base> _graphicButton;

        public IReadOnlyList<Button_Base> AntialiasingButton => _antialiasingButton;
        [SerializeField] List<Button_Base> _antialiasingButton;
        public TextMesh AntialiasingText => _antialiasingText;
        [SerializeField] TextMesh _antialiasingText;

        public IReadOnlyList<Button_Base> MSAAButton => _msaaButton;
        [SerializeField] List<Button_Base> _msaaButton;

        public TextMesh MSAAText => _msaaText;
        [SerializeField] TextMesh _msaaText;

        public IReadOnlyList<TextMesh> AntialiasingInfoText => _antialiasinginfoText;
        [SerializeField] List<TextMesh> _antialiasinginfoText;

        public IReadOnlyList<SliderGrabController> GraphicSlider => _graphicSlider;
        [SerializeField] List<SliderGrabController> _graphicSlider;

        public IReadOnlyList<TextMesh> GraphicsText => _graphicsText;
        [SerializeField] List<TextMesh> _graphicsText;

        void Awake()
        {
            Assert.IsNotNull(_graphicButton);
            Assert.IsNotNull(_antialiasingButton);
            Assert.IsNotNull(_antialiasingText);
            Assert.IsNotNull(_msaaButton);
            Assert.IsNotNull(_msaaText);
            Assert.IsNotNull(_antialiasinginfoText);
            Assert.IsNotNull(_graphicSlider);
            Assert.IsNotNull(_graphicsText);

            foreach (var button in _graphicButton)
            {
                Assert.IsNotNull(button);
            }
            foreach (var button in _antialiasingButton)
            {
                Assert.IsNotNull(button);
            }
            foreach (var button in _msaaButton)
            {
                Assert.IsNotNull(button);
            }
            foreach (var text in _antialiasinginfoText)
            {
                Assert.IsNotNull(text);
            }
            foreach (var slider in _graphicSlider)
            {
                Assert.IsNotNull(slider);
            }
            foreach (var text in _graphicsText)
            {
                Assert.IsNotNull(text);
            }
        }
    }
}