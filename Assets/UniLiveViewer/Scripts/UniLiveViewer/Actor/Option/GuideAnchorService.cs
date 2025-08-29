using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class GuideAnchorService
    {
        const string Path = "Prefabs/GuideAnchor/GuideBody";
        readonly int ColorId = Shader.PropertyToID("_Color");

        MeshRenderer _renderer;
        MaterialPropertyBlock _propertyBlock;
        ActorEntity _actorEntity;
        Color _initColor;

        readonly Transform _parent;

        [Inject]
        public GuideAnchorService(LifetimeScope lifetimeScope)
        {
            _parent = lifetimeScope.transform;
        }

        public void Setup()
        {
            if (_renderer != null) return;
            var go = GameObject.Instantiate(Resources.Load<GameObject>(Path), _parent);
            _renderer = go.GetComponent<MeshRenderer>();
            _renderer.transform.localPosition = Vector3.zero;

            _initColor = _renderer.material.GetColor(ColorId);
            _propertyBlock = new();
            _renderer.GetPropertyBlock(_propertyBlock);

            SetEnable(false);
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
        }

        public void OnPointerEnter()
        {
            _propertyBlock.SetColor(ColorId, Color.red);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void OnPointerExit()
        {
            _propertyBlock.SetColor(ColorId, _initColor);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetEnable(bool isEnable)
        {
            if (_renderer == null || _renderer.gameObject.activeSelf == isEnable) return;
            _renderer.gameObject.SetActive(isEnable);
        }

        public void OnTick()
        {
            if (_renderer == null || _renderer.gameObject.activeSelf == false) return;
            if (_actorEntity == null) return;
            var direction = _actorEntity.BoneMap[HumanBodyBones.Head].position - _renderer.transform.position;
            _renderer.transform.forward = direction;
        }

        public void Dispose()
        {
            if (_renderer.gameObject == null) return;
            GameObject.Destroy(_renderer.material);
            GameObject.Destroy(_renderer.gameObject);
        }
    }
}
