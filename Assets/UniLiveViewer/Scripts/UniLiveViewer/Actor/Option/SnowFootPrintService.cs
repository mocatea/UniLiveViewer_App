using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor.Option
{
    public class SnowFootprintService
    {
        readonly SnowFootpintSettings _settings;
        Transform _leftFoot;
        Transform _rightFoot;

        [Inject]
        public SnowFootprintService(SnowFootpintSettings settings)
        {
            _settings = settings;
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            if (actorEntity == null) return;
            Setup(actorEntity);
        }

        void Setup(ActorEntity actorEntity)
        {
            var map = actorEntity.BoneMap;
            _leftFoot = map[HumanBodyBones.LeftFoot];
            _rightFoot = map[HumanBodyBones.RightFoot];

            var lFootPoint = GameObject.Instantiate(_settings.SnowFootPointPrefab);
            var rFootPoint = GameObject.Instantiate(_settings.SnowFootPointPrefab);

            lFootPoint.SetParent(_leftFoot);
            lFootPoint.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            lFootPoint.localScale = Vector3.one * 0.5f;
            rFootPoint.SetParent(_rightFoot);
            rFootPoint.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            rFootPoint.localScale = Vector3.one * 0.5f;
        }
    }
}

