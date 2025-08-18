using UniLiveViewer.Actor;
using UnityEngine;

namespace UniLiveViewer.Timeline
{
    public class ShadowData
    {
        readonly int MainTexID = Shader.PropertyToID("_MainTex");

        public Transform spine;
        public Transform leftFoot;
        public Transform rightFoot;

        public MeshRenderer meshRenderer_c { get; }
        public MeshRenderer meshRenderer_l { get; }
        public MeshRenderer meshRenderer_r { get; }

        public ShadowData(MeshRenderer prefab, Transform parent)
        {
            meshRenderer_c = GameObject.Instantiate(prefab, parent);
            meshRenderer_l = GameObject.Instantiate(prefab, parent);
            meshRenderer_r = GameObject.Instantiate(prefab, parent);
        }

        public void SetBodyData(ActorEntity actorEntity)
        {
            var map = actorEntity.BoneMap;
            spine = map[HumanBodyBones.Spine];
            leftFoot = map[HumanBodyBones.LeftFoot];
            rightFoot = map[HumanBodyBones.RightFoot];
        }

        public void SetMeshRenderers(bool isEnable, Texture2D tex_body, Texture2D tex_foot)
        {
            meshRenderer_c.material.SetTexture(MainTexID, tex_body);
            meshRenderer_l.material.SetTexture(MainTexID, tex_foot);
            meshRenderer_r.material.SetTexture(MainTexID, tex_foot);

            meshRenderer_c.enabled = tex_body == null ? false : isEnable;
            meshRenderer_l.enabled = tex_foot == null ? false : isEnable;
            meshRenderer_r.enabled = tex_foot == null ? false : isEnable;
        }

        public void Dispose()
        {
            if (meshRenderer_c)
            {
                GameObject.Destroy(meshRenderer_c.material);
                GameObject.Destroy(meshRenderer_c.gameObject);
            }
            if (meshRenderer_l)
            {
                GameObject.Destroy(meshRenderer_l.material);
                GameObject.Destroy(meshRenderer_l.gameObject);
            }
            if (meshRenderer_r)
            {
                GameObject.Destroy(meshRenderer_r.material);
                GameObject.Destroy(meshRenderer_r.gameObject);
            }
        }
    }
}
