using TunnelEffect;
using UnityEngine;

namespace UniLiveViewer.Stage
{
    // TODO: service化、settingがあれば
    public class BackGroundController : MonoBehaviour
    {
        [SerializeField] int currntCubemap = 0;
        [SerializeField] int currntHole = 0;
        [SerializeField] int currntParticle = 0;

        [Header("＜キューブマップ＞")]
        [SerializeField] Material cubemap_Mat = null;
        public Material GetCubemapMat() { return cubemap_Mat; }
        [SerializeField] Cubemap[] cubemapList = null;

        [Header("＜トンネル＞")]
        [SerializeField] TunnelFX2 tunnelAnchor;
        [SerializeField] int[] tunnelList = new int[0];

        [Header("＜パーティクル＞")]
        [SerializeField] Transform[] particleAnchors = new Transform[0];

        void Start()
        {
            SetParticle(0);
            SetWormHole(0);
            SetCubemap(0);
        }

        public void SetParticle(int moveIndex)
        {
            currntParticle += moveIndex;
            if (particleAnchors.Length <= currntParticle) currntParticle = 0;
            else if (currntParticle < 0) currntParticle = particleAnchors.Length - 1;

            var setFlag = false;
            for (int i = 0; i < particleAnchors.Length; i++)
            {
                setFlag = (i == currntParticle);
                if (particleAnchors[i].gameObject.activeSelf != setFlag) particleAnchors[i].gameObject.SetActive(setFlag);
            }
        }

        public string GetParticleName(int moveIndex)
        {
            var temporaryIndex = currntParticle + moveIndex;
            if (particleAnchors.Length <= temporaryIndex) temporaryIndex = 0;
            else if (temporaryIndex < 0) temporaryIndex = particleAnchors.Length - 1;
            return temporaryIndex == 0 ? "None" : particleAnchors[temporaryIndex].name;
        }

        public void SetWormHole(int moveIndex)
        {
            currntHole += moveIndex;
            if (tunnelList.Length <= currntHole) currntHole = 0;
            else if (currntHole < 0) currntHole = tunnelList.Length - 1;

            if (currntHole == 0)
            {
                if (tunnelAnchor.gameObject.activeSelf) tunnelAnchor.gameObject.SetActive(false);
            }
            else
            {
                if (!tunnelAnchor.gameObject.activeSelf) tunnelAnchor.gameObject.SetActive(true);
                TunnelFX2.instance.preset = (TUNNEL_PRESET)tunnelList[currntHole];
            }
        }

        public string GetWormHolleName(int moveIndex)
        {
            var temporaryIndex = currntHole + moveIndex;
            if (tunnelList.Length <= temporaryIndex) temporaryIndex = 0;
            else if (temporaryIndex < 0) temporaryIndex = tunnelList.Length - 1;
            return temporaryIndex == 0 ? "None" : temporaryIndex.ToString();
        }

        /// <summary>
        /// passthrough用
        /// </summary>
        public void Clear_CubemapTex()
        {
            //cubemap_Mat.SetTexture("_Tex", null);
            //ワームホールを無効化しておく
            currntHole = 0;
            SetWormHole(0);
        }

        public void SetCubemap(int moveIndex)
        {
            currntCubemap += moveIndex;
            if (cubemapList.Length <= currntCubemap) currntCubemap = 0;
            else if (currntCubemap < 0) currntCubemap = cubemapList.Length - 1;

            cubemap_Mat.SetTexture("_Tex", cubemapList[currntCubemap]);
        }

        public string GetCubemapName(int moveIndex)
        {
            var temporaryIndex = currntCubemap + moveIndex;
            if (cubemapList.Length <= temporaryIndex) temporaryIndex = 0;
            else if (temporaryIndex < 0) temporaryIndex = cubemapList.Length - 1;
            return temporaryIndex.ToString();
        }
    }
}