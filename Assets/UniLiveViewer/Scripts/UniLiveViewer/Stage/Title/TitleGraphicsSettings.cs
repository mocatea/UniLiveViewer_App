using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UniLiveViewer.Stage.Title
{
    public class TitleGraphicsSettings : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        UniversalAdditionalCameraData _cameraData;
        UniversalRenderPipelineAsset _urpAsset;

        void Awake()
        {
            Assert.IsNotNull(_camera);
        }

        void Start()
        {
            _urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            _urpAsset.msaaSampleCount = (int)MSAASamples.MSAA4x;

            _cameraData = _camera.GetComponent<UniversalAdditionalCameraData>();
            _cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            _cameraData.antialiasingQuality = AntialiasingQuality.High;//SMAA専用設定
        }
    }
}