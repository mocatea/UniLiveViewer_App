using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Stage.Title
{
    public class TitleSceneSettings : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        [SerializeField] SpriteRenderer _spriteRenderer;

        public TextMesh AppVersionText => _appVersionText;
        [SerializeField] TextMesh _appVersionText;

        public OVRScreenFade OvrScreenFade => _ovrScreenFade;
        [SerializeField] OVRScreenFade _ovrScreenFade;

        public GameObject ScalingEffect => _scalingEffect;
        [SerializeField] GameObject _scalingEffect;

        void Awake()
        {
            Assert.IsNotNull(_spriteRenderer);
            Assert.IsNotNull(_appVersionText);
            Assert.IsNotNull(_ovrScreenFade);
            Assert.IsNotNull(_scalingEffect);
        }
    }
}