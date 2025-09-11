using System;
using UniRx;
using UnityEngine;

namespace UniLiveViewer.Player
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineSelector : MonoBehaviour
    {
        //ベジェ曲線用
        [Header("＜曲線の設定＞")]
        [SerializeField] Transform _lineStartAnchor = null;
        public Transform LineEndAnchor => _lineEndAnchor;
        [SerializeField] Transform _lineEndAnchor = null;
        Vector3 _endAnchorKeepEuler = Vector3.zero;
        [SerializeField] float _distance = 5.0f;
        [SerializeField] float _high = 1.5f;
        Vector3[] _bezierCurvePoint = new Vector3[3];
        float _bezierCurveTimer = 0;

        LineRenderer _lineRenderer = null;
        [SerializeField] int _positionCount = 20;

        //衝突検知
        [Header("＜衝突検知の設定＞")]
        [SerializeField] Transform _rayOrigin;
        [SerializeField] Vector3 _rayDirection = new Vector3(0, -1, 0);
        Transform _preHitObj;
        public IObservable<Transform> HitActorAsObservable => _hitActorStream;
        readonly Subject<Transform> _hitActorStream = new();

        PlayerHandState _handState = PlayerHandState.DEFAULT;

        void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            //LineRendererのパラメータ設定
            _lineRenderer.positionCount = _positionCount;
            //角度の初期値を取得
            _endAnchorKeepEuler = LineEndAnchor.localRotation.eulerAngles;
            //開幕無効化しておく
            gameObject.SetActive(false);
        }

        void Update()
        {
            UpdateBezierCurve();
            ChecFloorCollision();
            CheckActorCollision();

            var isForceReset = _handState != PlayerHandState.SUMMONCIRCLE;
        }

        void UpdateBezierCurve()
        {
            //ベジェ曲線の開始点、中間点、終了点を算出する
            _bezierCurvePoint[0] = _lineStartAnchor.position;
            _bezierCurvePoint[1] = _lineStartAnchor.position + (_lineStartAnchor.forward * _distance / _high);
            _bezierCurvePoint[2] = _lineStartAnchor.position + (_lineStartAnchor.forward * _distance);
            _bezierCurvePoint[2].y = transform.position.y;//一旦親の高さに揃える

            var pos = Vector3.zero;
            for (int i = 0; i < _lineRenderer.positionCount; i++)
            {
                //BezierCurveTimer:0～1
                _bezierCurveTimer = (float)i / (_lineRenderer.positionCount - 1);
                //ベジェ曲線の補完座標を取得
                pos = GetLerpPoint(_bezierCurvePoint[0], _bezierCurvePoint[1], _bezierCurvePoint[2], _bezierCurveTimer);
                _lineRenderer.SetPosition(i, pos);
            }
        }

        void ChecFloorCollision()
        {
            //床に向かってrayを飛ばす
            Physics.Raycast(_rayOrigin.position, _rayDirection, out var hitCollider, 3.0f, Constants.LayerMaskStageFloor);
            //Debug.DrawRay(rayOrigin.position, rayDirection, Color.red);
            //床の高さに合わせる
            if (hitCollider.collider) _bezierCurvePoint[2].y = hitCollider.point.y;

            //地面Anchor用のオブジェクトを移動する
            if (LineEndAnchor) LineEndAnchor.position = _bezierCurvePoint[2];
        }

        void CheckActorCollision()
        {
            //衝突検知(なるべく短くしてる)
            Physics.Raycast(LineEndAnchor.position, Vector3.up, out var hitCollider, 2.0f, Constants.LayerMaskFieldObject);
            //Physics.BoxCast(LineEndAnchor.position, Vector3.one * 0.1f ,Vector3.up, out var hitCollider, transform.rotation,1.5f, Constants.LayerMaskFieldObject);

            if (hitCollider.collider == null && _preHitObj != null)
            {
                _preHitObj = null;
                _hitActorStream.OnNext(null);
            }
            else if(hitCollider.collider != null && _preHitObj != hitCollider.collider.transform)
            {
                _preHitObj = hitCollider.collider.transform;
                _hitActorStream.OnNext(_preHitObj);
            }

            Debug.DrawRay(LineEndAnchor.position, Vector3.up, Color.red);
        }

        public void OnChangeHandState(PlayerHandState handState)
        {
            _handState = handState;
        }

        /// <summary>
        /// GroundPointerのオイラー角度を加算する
        /// </summary>
        public void GroundPointer_AddEulerAngles(Vector3 addAngles)
        {
            var eulerAngles = LineEndAnchor.localRotation.eulerAngles + addAngles;
            LineEndAnchor.localRotation = Quaternion.Euler(eulerAngles);
        }

        /// <summary>
        /// ベジェ曲線上の補間座標を返す
        /// </summary>
        Vector3 GetLerpPoint(Vector3 point0, Vector3 point1, Vector3 point2, float time)
        {
            var movePointA = Vector3.Lerp(point0, point1, time);
            var movePointB = Vector3.Lerp(point1, point2, time);
            var movePointC = Vector3.Lerp(movePointA, movePointB, time);

            return movePointC;
        }

        void OnEnable()
        {
            LineEndAnchor.localRotation = Quaternion.Euler(_endAnchorKeepEuler);
        }

        void OnDisable()
        {
            LineEndAnchor.localRotation = Quaternion.Euler(_endAnchorKeepEuler);
        }
    }
}