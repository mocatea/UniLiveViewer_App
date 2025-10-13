using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

namespace UniLiveViewer.Player
{
    public class PlayerGraphicsSettings : MonoBehaviour
    {
        public IReadOnlyList<UniversalRenderPipelineAsset> UrpAssets => _urpAssets;
        [SerializeField] List<UniversalRenderPipelineAsset> _urpAssets;

        [SerializeField] UniversalRendererData _frd;
        public Material OutlineMat => _outlineMat;
        [SerializeField] Material _outlineMat;

        public MobileFriendlyBloomFeature CustomBloomRenderFeature => _customBloomRenderFeature;
        [SerializeField] MobileFriendlyBloomFeature _customBloomRenderFeature;
        public ScriptableRendererFeature OutlineRenderFeature => _outlineRenderFeature;
        [SerializeField] ScriptableRendererFeature _outlineRenderFeature;

        void Awake()
        {
            Assert.IsNotNull(_urpAssets);
            Assert.IsNotNull(_frd);
            Assert.IsNotNull(_outlineMat);
            // _outlineRenderは動的に取得
            foreach (var urpAsset in _urpAssets)
            {
                Assert.IsNotNull(urpAsset);
            }

            //レンダーパイプラインからoutlineオブジェクトを取得    
            foreach (var rendererFeature in _frd.rendererFeatures)
            {
                if (rendererFeature.name == "MobileFriendlyBloomFeature"
                    && rendererFeature is MobileFriendlyBloomFeature bloom)
                {
                    _customBloomRenderFeature = bloom;
                }
                else if (rendererFeature.name == "Outline")
                {
                    _outlineRenderFeature = rendererFeature;
                }
            }
            _outlineRenderFeature.SetActive(false);
        }
    }
}