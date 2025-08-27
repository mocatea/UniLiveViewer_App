using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UniLiveViewer.Actor.Option
{
    public class DecalShadow : IFakeShadow
    {
        const string PrefabPath = "Prefabs/Shadow/DecalShadow";
        const float ActorBaseSize = 1.5f;
        readonly int _mainTexID = Shader.PropertyToID("_MainTex");
        readonly int _alphaID = Shader.PropertyToID("_Alpha");
        readonly List<(Transform parentBone, DecalProjector shadow)> _map = new();

        bool _isEnable;
        float _rootScalar;
        float _userShadowScale;
        ActorEntity _actorEntity;

        public void Setup(Transform parent, SHADOWTYPE shadowType, QuasiShadowSetting.Preset preset, float userShadowScale)
        {
            _userShadowScale = userShadowScale;

            var prefab = Resources.Load<GameObject>(PrefabPath);
            var templateMat = prefab.GetComponent<DecalProjector>().material;

            for (int i = 0; i < 3; i++)
            {
                var go = GameObject.Instantiate(prefab, parent);
                var shadow = go.GetComponent<DecalProjector>();
                shadow.material = Object.Instantiate(templateMat);// パラメータ込みで複製
                _map.Add((null, shadow));
            }

            OnUpdateShadowType(shadowType, preset);
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
            if (actorEntity == null) return;

            var boneMap = actorEntity.BoneMap;
            _map[0] = (boneMap[HumanBodyBones.Spine], _map[0].shadow);
            _map[1] = (boneMap[HumanBodyBones.LeftFoot], _map[1].shadow);
            _map[2] = (boneMap[HumanBodyBones.RightFoot], _map[2].shadow);
        }

        public void SetEnable(bool isEnable)
        {
            _isEnable = isEnable;

            if (_actorEntity == null) return;

            for (int i = 0; i < _map.Count; i++)
            {
                _map[i].shadow.gameObject.SetActive(isEnable);
            }
        }

        public void OnChangeRootScalar(float rootScalar) => _rootScalar = rootScalar;


        public void OnUpdateShadowType(SHADOWTYPE shadowType, QuasiShadowSetting.Preset preset)
        {
            _isEnable = shadowType != SHADOWTYPE.NONE;
            for (int i = 0; i < _map.Count; i++)
            {
                _map[i].shadow.gameObject.SetActive(_isEnable);
            }

            if (_isEnable)
            {
                _map[0].shadow.material.SetTexture(_mainTexID, preset.texture_Body);
                _map[1].shadow.material.SetTexture(_mainTexID, preset.texture_Foot);
                _map[2].shadow.material.SetTexture(_mainTexID, preset.texture_Foot);
            }
        }

        public void OnUpdate(QuasiShadowSetting.Preset preset, QuasiShadowSetting setting)
        {
            if (_actorEntity == null || !_isEnable) return;

            Transforming(_map[0].parentBone, _map[0].shadow, preset.scala_Body, setting.BodyAttenuationMultiplier);
            Transforming(_map[1].parentBone, _map[1].shadow, preset.scala_Foot, setting.FootAttenuationMultiplier);
            Transforming(_map[2].parentBone, _map[2].shadow, preset.scala_Foot, setting.FootAttenuationMultiplier);
        }

        void Transforming(Transform targetBone, DecalProjector shadow, float settingsScala, float attenuationMultiplier)
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

            shadow.transform.position = shadowPos;
            shadow.transform.localScale = Vector3.one * baseScale * shadowScaleFactor;
            shadow.material.SetFloat(_alphaID, shadowScaleFactor);
        }

        public void Dispose()
        {
            for (int i = 0; i < _map.Count; i++)
            {
                GameObject.Destroy(_map[i].shadow.gameObject);
                GameObject.Destroy(_map[i].shadow.material);
            }
            _map.Clear();
        }
    }
}