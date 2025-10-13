using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace UniLiveViewer
{
    /// <summary>
    /// 一旦Rxしない
    /// </summary>
    public class Button_Base : MonoBehaviour
    {
        public bool isEnable
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value;
                if (collisionChecker)
                {
                    //強制的に親の状態に揃える
                    if (_isEnable) collisionChecker.myState = SWITCHSTATE.ON;
                    else collisionChecker.myState = SWITCHSTATE.OFF;
                }
            }
        }
        //好きな方使う(パフォーマンスとトレードオフ)
        public event Action<Button_Base> onTrigger;
        public UnityEvent onTrigger_Event;

        [SerializeField] protected float delayTime = 1.0f;

        //Parameter
        public CollisionChecker collisionChecker = null;
        [SerializeField] private Transform neutralAnchor = null;

        [Header("確認用露出(readonly)")]
        [SerializeField] protected bool _isEnable = true;

        protected Rigidbody myRb;
        protected BoxCollider myCol;

        protected CancellationToken cancellation_token;

        protected virtual void Awake()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();

            collisionChecker.Init();
            myRb = collisionChecker.GetComponent<Rigidbody>();
            myCol = collisionChecker.GetComponent<BoxCollider>();
        }

        void OnEnable()
        {
            InitDirecting().Forget();
        }

        void FixedUpdate()
        {
            //バネ運動処理
            AddSpringForce(1500);
            AddSpringForceExtra();
        }

        private void LateUpdate()
        {
            if (myRb.isKinematic) return;
            //ボタンの移動範囲制限
            if (collisionChecker.transform.localPosition.z >= 0.0f)
            {
                //ボタンに触れていれば
                if (collisionChecker.Touching) ClickAction();
            }
        }

        /// <summary>
        /// クリック処理
        /// </summary>
        public void ClickAction()
        {
            //isTrigger = true;
            if (!gameObject.activeSelf) return;

            ClickDirecting().Forget();

            //押したと判定する
            onTrigger?.Invoke(this);
            onTrigger_Event?.Invoke();
        }

        /// <summary>
        /// クリック演出
        /// </summary>
        protected virtual async UniTaskVoid ClickDirecting()
        {
            //何度も押せないように物理判定を消す
            myRb.isKinematic = true;
            myCol.enabled = false;

            //フラグ初期化する
            isEnable = true;

            //座標初期化
            collisionChecker.transform.localPosition = Vector3.zero;

            //振動処理
            if (collisionChecker.IsTouchL) ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, 1, 0.1f);
            else ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, 1f, 0.1f);

            //押した後のインターバル
            await UniTask.Delay((int)(delayTime * 1000), cancellationToken: cancellation_token);

            //張り付かないように少し前進
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;

            //物理演算を再開し、触れられるように戻す 
            myRb.isKinematic = false;
            myCol.enabled = true;
        }

        /// <summary>
        /// テキストメッシュに指定文字列を設定
        /// </summary>
        public void SetTextMesh(string str)
        {
            if (collisionChecker.ColorSetting == null || !collisionChecker.ColorSetting[0].textMesh) return;
            collisionChecker.ColorSetting[0].textMesh.text = str;
        }

        private async UniTaskVoid InitDirecting()
        {
            await UniTask.Yield(cancellation_token);

            //押せないように物理判定を消す
            myRb.isKinematic = true;
            myCol.enabled = false;

            //座標初期化
            collisionChecker.transform.localPosition = Vector3.zero;

            //インターバル
            await UniTask.Delay((int)(delayTime * 1000), cancellationToken: cancellation_token);

            //張り付かないように少し前進
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;

            //物理演算を再開し、触れられるように戻す 
            myRb.isKinematic = false;
            myCol.enabled = true;
        }

        /// <summary>
        /// バネの力を加える
        /// </summary>
        /// <param バネ係数="r"></param>
        private void AddSpringForce(float r)
        {
            var diff = neutralAnchor.position - collisionChecker.transform.position; //バネの伸び
            var force = diff * r;
            myRb.AddForce(force);
        }

        /// <summary>
        /// オーバーシュートしないバネの力を加える
        /// </summary>
        private void AddSpringForceExtra()
        {
            var r = myRb.mass * myRb.drag * myRb.drag / 4f;//バネ係数
            var diff = neutralAnchor.position - collisionChecker.transform.position; //バネの伸び
            var force = diff * r;
            myRb.AddForce(force);
        }
    }
}