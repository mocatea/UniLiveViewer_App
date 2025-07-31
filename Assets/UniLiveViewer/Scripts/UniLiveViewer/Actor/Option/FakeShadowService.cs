using System;
using UniLiveViewer.Actor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    /// <summary>
    /// TODO:生成の仕方変えたい
    /// </summary>
    public class FakeShadowService : IDisposable
    {
        const string Path = "Prefabs/Shadow/ShadowPrefab";

        bool _isEnable;
        float _rootScalar;
        ShadowData _shadowData;
        ActorEntity _actorEntity;
        SHADOWTYPE _shadowType;
        QuasiShadowSetting.Preset _preset;
        float _shadowScale;

        readonly LifetimeScope _parent;

        [Inject]
        public FakeShadowService(LifetimeScope lifetimeScope)
        {
            _parent = lifetimeScope;
        }

        public void Setup(SHADOWTYPE shadowType, float shadowScale, QuasiShadowSetting.Preset preset)
        {
            _shadowType = shadowType;
            _shadowScale = shadowScale;
            _preset = preset;

            var meshRenderer = GameObject.Instantiate<MeshRenderer>(Resources.Load<MeshRenderer>(Path));

            //メッシュ消え対策 ←Shader移動じゃなければ多分不要
            //var meshFilter = meshRenderer.GetComponent<MeshFilter>();
            //var bounds = meshFilter.mesh.bounds;
            //bounds.Expand(100);
            //meshFilter.mesh.bounds = bounds;

            _shadowData = new ShadowData(meshRenderer, _parent.transform);
            _shadowData.SetMeshRenderers(false, null, null);
            SetEnable(false);

            GameObject.Destroy(meshRenderer.gameObject);

            // 初期反映
            UpdateMeshRenderers();
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
            if (actorEntity == null) return;
            _shadowData.SetBodyData(_actorEntity);
        }

        public void SetEnable(bool isEnable)
        {
            _isEnable = isEnable;

            if (_shadowData == null) return;
            _shadowData.meshRenderer_c.gameObject.SetActive(isEnable);
            _shadowData.meshRenderer_l.gameObject.SetActive(isEnable);
            _shadowData.meshRenderer_r.gameObject.SetActive(isEnable);
        }

        public void OnChangeRootScalar(float rootScalar)
        {
            _rootScalar = rootScalar;
        }

        public void OnUpdateShadowSettings(SHADOWTYPE shadowType, float shadowScale, QuasiShadowSetting.Preset preset)
        {
            _shadowType = shadowType;
            _shadowScale = shadowScale;
            _preset = preset;
            UpdateMeshRenderers();
        }

        void UpdateMeshRenderers()
        {
            var isEnable = _shadowType != SHADOWTYPE.NONE;
            _shadowData.SetMeshRenderers(isEnable, _preset.texture_Body, _preset.texture_Foot);
        }

        public void OnTick()
        {
            if (_actorEntity == null || !_isEnable) return;
            if (_shadowType == SHADOWTYPE.NONE) return;
            Transforming(_shadowData.spine, _shadowData.meshRenderer_c, _preset.scala_Body);
            Transforming(_shadowData.leftFoot, _shadowData.meshRenderer_l, _preset.scala_Foot);
            Transforming(_shadowData.rightFoot, _shadowData.meshRenderer_r, _preset.scala_Foot);
        }

        void Transforming(Transform targetBone, MeshRenderer targetMesh, float presetScale)
        {
            var scale = presetScale * _shadowScale * _rootScalar;
            var offset = targetBone.position;
            var actorYPos = _actorEntity.GetAnimator.transform.position.y;
            offset.y = actorYPos;
            var distance = (targetBone.position.y - actorYPos) / _rootScalar;

            //親オブジェ変わって無理になったので一旦Shader辞め
            //targetMesh.material.SetVector("_Position", offset);
            //targetMesh.material.SetFloat("_Scale", scale * (1 - (distance * 0.4f)));
            //targetMesh.material.SetFloat("_Alpha", 1 - (distance * 0.5f));

            targetMesh.transform.position = offset;
            targetMesh.transform.localScale = Vector3.one * scale * (1 - (distance * 0.4f));
            targetMesh.material.SetFloat("_Alpha", 1 - (distance * 0.5f));
        }

        public void Dispose()
        {
            _shadowData.Dispose();
        }
    }

    public class ShadowData
    {
        const string TEXTURE_NAME = "_MainTex";

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
            meshRenderer_c.material.SetTexture(TEXTURE_NAME, tex_body);
            meshRenderer_l.material.SetTexture(TEXTURE_NAME, tex_foot);
            meshRenderer_r.material.SetTexture(TEXTURE_NAME, tex_foot);

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
