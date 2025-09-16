using UnityEngine;
using VContainer.Unity;
using VContainer;
using UniLiveViewer.Timeline;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class ChaserLight : MonoBehaviour, IStageLight
    {
        const HumanBodyBones TargetHumanBodyBone = HumanBodyBones.Spine;
        const string PropertyName = "_TintColor";

        [SerializeField] MeshRenderer[] _lights;
        [SerializeField] AnimationCurve _colorCurveR = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _colorCurveG = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _colorrCurveB = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] float _colorSpeed = 1;

        bool _isWhitelight = true;
        float _colorTimer = 0;
        Vector3 _pos;

        Transform[] _targetBones;
        PlayableBinderService _playableBinderService;

        /// <summary>
        /// hierarchy上の方にしたら多分container間に合わないので注意
        /// </summary>
        void Awake()
        {
            _targetBones = new Transform[SystemInfo.MaxFieldActor];

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

            for (int i = 0; i < _targetBones.Length; i++)
            {
                var data = _playableBinderService.BindingData[i + 1];
                if (data == null) _targetBones[i] = null;
                else
                {
                    _targetBones[i] = data.ActorEntity.ActorEntity().Value.BoneMap[TargetHumanBodyBone];
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
            for (int i = 0; i < _targetBones.Length; i++)
            {
                if (!_targetBones[i]) continue;
                _pos = _targetBones[i].position;
                _pos.y = _lights[i].transform.position.y;
                _lights[i].transform.position = _pos;
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
                        _colorrCurveB.Evaluate(_colorTimer)
                        )
                    );
            }
            _colorTimer += Time.deltaTime * _colorSpeed;
            if (_colorTimer > 1.05f) _colorTimer = 0;
        }
    }
}