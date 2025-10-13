using System.Collections.Generic;
using UniLiveViewer.Timeline;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class LookAtLight : MonoBehaviour, IStageLight
    {
        const HumanBodyBones TargetHumanBodyBone = HumanBodyBones.Spine;
        const string PropertyName = "_TintColor";

        [SerializeField] MeshRenderer[] _lights;
        [SerializeField] AnimationCurve _colorCurveR = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _colorCurveG = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _colorCurveB = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] float _colorSpeed = 1;

        bool _isWhitelight = true;
        float _colorTimer = 0;
        Vector3 _distance;

        Transform[] _targetBones;
        List<Transform> _targetList;
        PlayableBinderService _playableBinderService;

        /// <summary>
        /// hierarchy上の方にしたら多分container間に合わないので注意
        /// </summary>
        void Awake()
        {
            _targetBones = new Transform[SystemInfo.MaxFieldActor];
            _targetList = new List<Transform>();

            var container = LifetimeScope.Find<TimelineLifetimeScope>().Container;
            _playableBinderService = container.Resolve<PlayableBinderService>();
        }

        void IStageLight.ChangeCount(int count)
        {
            for (int i = 0; i < _lights.Length; i++)
            {
                var enable = i < count;
                if (_lights[i].gameObject.activeSelf == enable) continue;
                _lights[i].gameObject.SetActive(enable);
            }

            _targetList.Clear();
            for (int i = 0; i < _targetBones.Length; i++)
            {
                var data = _playableBinderService.BindingData[i + 1];
                if (data == null) _targetBones[i] = null;
                else
                {
                    _targetBones[i] = data.ActorEntity.ActorEntity().Value.BoneMap[TargetHumanBodyBone];
                    _targetList.Add(_targetBones[i]);
                }
            }
        }

        void IStageLight.ChangeColor(bool isWhite)
        {
            _isWhitelight = isWhite;
            if (!_isWhitelight) return;

            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].sharedMaterial.SetColor(PropertyName, Color.white);
            }
        }

        void IStageLight.OnUpdate()
        {
            for (int i = 0; i < _targetList.Count; i++)
            {
                _distance = _targetList[i].position - _lights[i].transform.position;
                _lights[i].transform.up = _distance;
            }
            UpdateColor();
        }

        void UpdateColor()
        {
            if (_isWhitelight) return;

            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].sharedMaterial.SetColor
                    (PropertyName,
                    new Color(
                        _colorCurveR.Evaluate(_colorTimer),
                        _colorCurveG.Evaluate(_colorTimer),
                        _colorCurveB.Evaluate(_colorTimer)
                        )
                    );
            }
            _colorTimer += Time.deltaTime * _colorSpeed;
            if (_colorTimer > 1.05f) _colorTimer = 0;
        }
    }
}