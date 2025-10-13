using System;
using UniLiveViewer.OVRCustom;
using UniRx;
using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// TOOD: 完全に作り直す
    /// </summary>
    public class DecorationItemInfo : MonoBehaviour
    {
        // TODO: ローカライズUnity標準にする
        public string[] ItemName => itemName;
        [SerializeField] string[] itemName = new string[2] { "アイテム名", "ItemName" };

        public RenderInfo[] RenderInfo => renderInfo;
        [SerializeField] RenderInfo[] renderInfo = new RenderInfo[0];

        [SerializeField] string[] flavorText = new string[2] { "何の変哲もないアイテム", "Unremarkable item" };//未使用
        OVRGrabbableCustom _ovrGrabbableCustom;

        public IObservable<bool> AttachedAsObservable => _attachedStream;
        readonly Subject<bool> _attachedStream = new();

        MeshRenderer _meshRenderer;
        bool _isAttached;

        void Awake()
        {
            _ovrGrabbableCustom = GetComponent<OVRGrabbableCustom>();
            _meshRenderer = transform.GetComponent<MeshRenderer>();

            _ovrGrabbableCustom.GrabBeginAsObservable
                .Subscribe(x => OnGrabbed(x.transform)).AddTo(this);
            void OnGrabbed(Transform parent)
            {
                transform.parent = parent;
                _meshRenderer.enabled = true;
                _isAttached = false;
            }
        }

        /// <summary>
        /// 指定テクスチャに変更
        /// </summary>
        public void SetTexture(int userSelectIndex)
        {
            int i = 0;//現状は0しかないので固定
            int matIndex = renderInfo[i].data.materialIndex;
            renderInfo[i].data.textureCurrent = userSelectIndex;

            if (!renderInfo[i].data.convertToColor)
            {
                foreach (var renderer in renderInfo[i]._renderers)
                {
                    foreach (var shaderName in renderInfo[i].data.targetShaderName)
                    {
                        renderer.materials[matIndex].SetTexture(
                            shaderName,
                            renderInfo[i].data.chooseableTexture[renderInfo[i].data.textureCurrent]);
                    }
                }
            }
            else
            {
                var itemColorChanger = transform.GetChild(0).GetComponent<IItemColorChanger>();
                if (itemColorChanger == null) return;

                // ex: tex_sk
                var textureName = renderInfo[i].data.chooseableTexture[renderInfo[i].data.textureCurrent].name;
                var parts = textureName.Split('_');
                var result = parts.Length > 1 ? parts[1] : string.Empty;
                var colorInfo = result.ToColorInfo();
                foreach (var shaderName in renderInfo[i].data.targetShaderName)
                {
                    itemColorChanger.SetColor(shaderName, colorInfo);
                }
            }
        }

        /// <summary>
        /// TODO: これをここでやってるのもそもそも変だがLS化しないと厳しい
        /// </summary>
        public bool TryAttachment()
        {
            var collider = _ovrGrabbableCustom.HitCollider;
            if (!collider) return false;
            //アタッチする
            _ovrGrabbableCustom.transform.parent = collider.transform;
            _meshRenderer.enabled = false;
            _isAttached = true;
            _attachedStream.OnNext(true);
            return true;
        }

        void OnDestroy()
        {
            if (_isAttached) return;

            for (int i = 0; i < renderInfo.Length; i++)
            {
                for (int j = 0; j < renderInfo[i]._renderers.Length; j++)
                {
                    for (int k = 0; k < renderInfo[i]._renderers[j].materials.Length; k++)
                    {
                        if (!renderInfo[i]._renderers[j].materials[k]) continue;
                        Destroy(renderInfo[i]._renderers[j].materials[k]);
                    }
                }
            }

            // MEMO: Linq
            //var materials = renderInfo
            //    .SelectMany(info => info._renderers)
            //    .SelectMany(renderer => renderer.materials)
            //    .Where(material => material != null);
            //foreach (var material in materials)
            //{
            //    Destroy(material);
            //}

            renderInfo = null;
        }
    }

    [System.Serializable]
    public class RenderInfo
    {
        public RenderInfoData data;
        public Renderer[] _renderers;
    }
}