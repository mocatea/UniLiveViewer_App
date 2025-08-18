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
        const float ActorBaseSize = 1.5f;
        const string Path = "Prefabs/Shadow/ShadowPrefab";
        readonly int _alphaID = Shader.PropertyToID("_Alpha");

        bool _isEnable;
        float _rootScalar;
        ShadowData _shadowData;
        ActorEntity _actorEntity;
        SHADOWTYPE _shadowType;
        QuasiShadowSetting _settings;
        QuasiShadowSetting.Preset _preset;
        float _userShadowScale;

        readonly LifetimeScope _parent;

        [Inject]
        public FakeShadowService(LifetimeScope lifetimeScope)
        {
            _parent = lifetimeScope;
        }

        public void Setup(SHADOWTYPE shadowType, float userShadowScale, QuasiShadowSetting settings, int presetIndex)
        {
            _shadowType = shadowType;
            _userShadowScale = userShadowScale;
            _settings = settings;
            _preset = settings.Presets[presetIndex];

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

        public void OnUpdateShadowSettings(SHADOWTYPE shadowType, float userShadowScale, QuasiShadowSetting settings, int presetIndex)
        {
            _shadowType = shadowType;
            _userShadowScale = userShadowScale;
            _settings = settings;
            _preset = settings.Presets[presetIndex];
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

            Transforming(_shadowData.spine, _shadowData.meshRenderer_c, _preset.scala_Body, _settings.BodyAttenuationMultiplier);
            Transforming(_shadowData.leftFoot, _shadowData.meshRenderer_l, _preset.scala_Foot, _settings.FootAttenuationMultiplier);
            Transforming(_shadowData.rightFoot, _shadowData.meshRenderer_r, _preset.scala_Foot, _settings.FootAttenuationMultiplier);
        }

        void Transforming(Transform targetBone, MeshRenderer targetMesh, float settingsScala, float attenuationMultiplier)
        {
            const float minShadowScale = 0f; // 足を最大に上げてもここまで

            // 座標
            var shadowPos = targetBone.position;
            var baseY = _actorEntity.GetAnimator.transform.position.y;
            shadowPos.y = baseY;

            // 上げ量に応じた係数
            var liftFactor = Mathf.Max(0f, targetBone.position.y - baseY);
            liftFactor = Mathf.Clamp01(liftFactor / _rootScalar) * attenuationMultiplier;
            // 上げるほど小さく
            var shadowScaleFactor = Mathf.Lerp(1f, minShadowScale, liftFactor);
            shadowScaleFactor = Mathf.Clamp(shadowScaleFactor, 0, 1f);

            var actorSizeCorrection = _actorEntity.Height / ActorBaseSize;
            var baseScale = _userShadowScale * settingsScala * actorSizeCorrection;
            Debug.Log($"baseScale:{baseScale} = _userShadowScale:{_userShadowScale} * actorSizeCorrection:{actorSizeCorrection}");

            targetMesh.transform.position = shadowPos;
            targetMesh.transform.localScale = Vector3.one * baseScale * shadowScaleFactor;
            targetMesh.material.SetFloat(_alphaID, shadowScaleFactor);
        }

        public void Dispose()
        {
            _shadowData.Dispose();
        }
    }
}
