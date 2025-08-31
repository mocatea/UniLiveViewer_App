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
        public ScriptableRendererFeature OutlineRender => _outlineRender;
        [SerializeField] ScriptableRendererFeature _outlineRender;

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
            foreach (var renderObj in _frd.rendererFeatures)
            {
                if (renderObj.name == "Outline")
                {
                    _outlineRender = renderObj;
                    break;
                }
            }
            _outlineRender.SetActive(false);
        }
    }
}