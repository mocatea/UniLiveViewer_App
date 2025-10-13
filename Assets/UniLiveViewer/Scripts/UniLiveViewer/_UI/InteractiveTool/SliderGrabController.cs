using System;
using UniLiveViewer.OVRCustom;
using UnityEngine;
using UniRx;

namespace UniLiveViewer
{
    /// <summary>
    /// 一旦Rxしない
    /// </summary>
    public class SliderGrabController : MonoBehaviour
    {
        readonly Quaternion _handAdjustment = Quaternion.Euler(new Vector3(0, 0, 180));
        
        /// <summary>
        /// 操作開始
        /// </summary>
        public IObservable<Unit> BeginDriveAsObservable => _beginDriveStream;
        readonly Subject<Unit> _beginDriveStream = new();
        /// <summary>
        /// 操作終了
        /// </summary>
        public IObservable<Unit> EndDriveAsObservable => _endDriveStream;
        readonly Subject<Unit> _endDriveStream = new();
        /// <summary>
        /// スライダー更新
        /// </summary>
        public IObservable<float> ValueAsObservable => _valueStream;
        readonly Subject<float> _valueStream = new();

        [Header("--- 設定 ---")]
        public Transform visibleHandler;
        [SerializeField] private Transform startAnchor;
        [SerializeField] private Transform endAnchor;
        [SerializeField] private Transform[] handMesh = new Transform[2];
        [Tooltip("VisibleHandleの子オブジェクトを指定")]
        [SerializeField] private OVRGrabbableCustom unVisibleHandler = null;

        public (float min, float max) GetMaxMinValuels => (minValuel, maxValuel);
        public void SetMaxValuel(float v) => maxValuel = v;
        [SerializeField] float maxValuel = 1.0f;
        [SerializeField] float minValuel = 0.0f;
        [SerializeField] float minStepValuel = 0.1f;//スライダーを動かす間隔
        [SerializeField] private bool SkipMoveMode = false;

        [Header("--- 確認 ---")]
        [SerializeField] float _value = 0;

        /// <summary>
        /// スライダーを握って操作されているか
        /// </summary>
        public bool IsGrabbed => _isGrabbed;
        bool _isGrabbed = false;

        /// <summary>
        /// Awake前にValue変更されることがある
        /// </summary>
        Vector3 _nextHandllocalPos = Vector3.zero;
        Vector3 _axis = Vector3.zero;
        float _handleMaxRangeX = 0;
        float _coefficient;
        bool _isGrabbedByLeftHand = false;

        /// <summary>
        /// ハンドルに指定したオブジェクトを範囲内で制御する(0～1)
        /// </summary>
        public float Value
        {
            get { return _value; }
            set
            {
                _value = Mathf.Clamp(value, minValuel, maxValuel);
                _valueStream.OnNext(_value);
            }
        }

        /// <summary>
        /// 掴み操作以外で動かす必要があるケースで利用
        /// NOTE: 通知すると無限ループに陥る
        /// </summary>
        public void SetValueWithoutNotify(float value)
        {
            _value = Mathf.Clamp(value, minValuel, maxValuel);
        }

        void Awake()
        {
            _handleMaxRangeX = endAnchor.localPosition.x - startAnchor.localPosition.x;

            unVisibleHandler.GrabBeginAsObservable
                .Subscribe(OnGrab).AddTo(this);
            unVisibleHandler.GrabEndAsObservable
                .Subscribe(OnRelease).AddTo(this);

            void OnGrab(OVRGrabbable grabbable)
            {
                if (_isGrabbed) return;
                InitializationOnGrab();
                _beginDriveStream.OnNext(Unit.Default);
            }

            void OnRelease(OVRGrabbable grabbable)
            {
                if (!_isGrabbed) return;
                InitializationOnRelease();
                _endDriveStream.OnNext(Unit.Default);
            }
        }

        void OnEnable()
        {
            InitializationOnRelease();
        }

        void OnDisable()
        {
            InitializationOnRelease();
        }

        void Start()
        {
            //ハンドルの初期化
            InitializationOnRelease();
            //係数決定
            _coefficient = maxValuel / minStepValuel / 2;
        }

        /// <summary>
        /// 離された時の初期化
        /// </summary>
        void InitializationOnRelease()
        {
            unVisibleHandler.transform.parent = visibleHandler;
            unVisibleHandler.transform.localPosition = Vector3.zero;

            handMesh[0].parent.transform.localRotation = Quaternion.identity;

            //UI用handを非表示に
            if (handMesh[0].gameObject.activeSelf) handMesh[0].gameObject.SetActive(false);
            if (handMesh[1].gameObject.activeSelf) handMesh[1].gameObject.SetActive(false);

            _isGrabbed = false;
        }

        /// <summary>
        /// 掴まれたときの初期化
        /// </summary>
        void InitializationOnGrab()
        {
            //捕まれていなかったら異常
            if (!unVisibleHandler.isGrabbed)
            {
                Debug.LogError("slider not grabbed.");
                return;
            }

            unVisibleHandler.transform.parent = null;//必須

            //実際の手を非表示
            var playerHand = (OVRGrabber_UniLiveViewer)unVisibleHandler.grabbedBy;
            playerHand.handMeshRoot.gameObject.SetActive(false);

            if (playerHand.name.Contains("HandL"))
            {
                if ((playerHand.transform.right).y <= 0)
                {
                    handMesh[0].parent.transform.localRotation *= _handAdjustment;
                }
                _isGrabbedByLeftHand = true;
                handMesh[0].gameObject.SetActive(true);//UI用の手を表示
            }
            else if (playerHand.name.Contains("HandR"))
            {
                if ((-playerHand.transform.right).y <= 0)
                {
                    handMesh[1].parent.transform.localRotation *= _handAdjustment;
                }
                _isGrabbedByLeftHand = false;
                handMesh[1].gameObject.SetActive(true);//UI用の手を表示
            }

            _isGrabbed = true;
        }

        void Update()
        {
            if (maxValuel <= 0) return;

            if (_isGrabbed) UpdateByPlayerGrab();
            UpdateSliderPosition();
        }

        /// <summary>
        /// Playerに握られているマニュアル更新状態
        /// </summary>
        void UpdateByPlayerGrab()
        {
            var direction = unVisibleHandler.transform.position - visibleHandler.position;
            _axis = Vector3.Cross(visibleHandler.forward, direction);
            var abs = Mathf.Abs(_axis.y);
            //滑らかに動く
            if (SkipMoveMode && abs >= 0.08f)
            {
                Value = _value + (_coefficient * _axis.y * Time.deltaTime);

                var touch = _isGrabbedByLeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
                ControllerVibration.Execute(touch, 1, 0.2f, 0.05f);
            }
            //Min設定値で刻む
            else if (abs >= 0.02f)
            {
                Value = _value + (Mathf.Sign(_axis.y) * minStepValuel);

                var touch = _isGrabbedByLeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
                ControllerVibration.Execute(touch, 1, 0.4f, 0.05f);
            }
        }

        void UpdateSliderPosition()
        {
            _nextHandllocalPos.x = _handleMaxRangeX * _value / maxValuel;
            visibleHandler.localPosition = startAnchor.localPosition + _nextHandllocalPos;
        }
    }
}
