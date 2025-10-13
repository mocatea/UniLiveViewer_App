using UnityEngine;
using UnityEngine.UI;

namespace UniLiveViewer
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionChecker : MonoBehaviour
    {
        [SerializeField] SWITCHSTATE _myState = SWITCHSTATE.ON;
        public bool Touching => _isTouch;
        bool _isTouch = false;
        public bool IsTouchL => _isTouchL;
        [SerializeField] bool _isTouchL;
        public SWITCHSTATE myState
        {
            get
            {
                return _myState;
            }
            set
            {
                //ボタン状態に応じて色を更新
                _myState = value;
                _isTouch = false;
                UpdateColor();
            }
        }
        public TargetColorSetting[] ColorSetting => colorSetting;
        //色の設定
        [SerializeField] TargetColorSetting[] colorSetting;

        // MEMO: 初回OnEnableだと上手くいかないので同じ事している
        void Start()
        {
            _isTouch = false;
            UpdateColor();
        }

        void OnEnable()
        {
            _isTouch = false;
            UpdateColor();
        }

        public void Init()
        {
            for (int i = 0; i < colorSetting.Length; i++)
            {
                colorSetting[i].Init(Constants.btnColor_Ena_sky);
            }
        }

        /// <summary>
        /// 状態に応じて色を更新
        /// </summary>
        void UpdateColor()
        {
            if (colorSetting == null) return;

            for (int i = 0; i < colorSetting.Length; i++)
            {
                colorSetting[i].SetColor(_myState, _isTouch);
            }
        }

        //Enterした次のフレームでExitするとStayは呼ばれないらしい
        //だがExitもすり抜けている気がするので、Stayさせれば確実にExitが発生するかも？なのでStayを使う
        void OnCollisionStay(Collision collision)
        {
            if (_isTouch) return;
            _isTouch = true;
            UpdateColor();

            //ヒット対象
            if (collision.transform.name.Contains("Left")) _isTouchL = true;
            else _isTouchL = false;

            //振動処理
            if (_isTouchL) ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, 0.6f, 0.05f);
            else ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, 0.6f, 0.05f);
        }

        /// <summary>
        /// 離れた時
        /// </summary>
        /// <param name="collision"></param>
        void OnCollisionExit(Collision collision)
        {
            if (!_isTouch) return;
            _isTouch = false;
            UpdateColor();
        }
    }

    /// <summary>
    /// 色情報を管理するクラス
    /// </summary>
    [System.Serializable]
    public class TargetColorSetting
    {
        //TODO:ダサ...でもいい方法知らない
        [Header("どれか1種にアタッチ")]
        public Image targetImage;
        public SpriteRenderer targetSprite;
        public MeshRenderer meshRender;
        public TextMesh textMesh;
        [Space(20)]
        [Tooltip("無効状態カラー")]
        public Color DisableColor = new Color(0.3f, 0.3f, 0.3f);
        [Tooltip("有効状態カラー")]
        public Color EnableColor = new Color(0.7f, 0.7f, 0.7f);
        [Tooltip("触れている状態カラー")]
        public Color TouchColor = new Color(0.7f, 0.7f, 0.3f);
        //[System.NonSerialized]
        private DRAWTYPE drawType = DRAWTYPE.NULL;

        private MaterialPropertyBlock materialPropertyBlock;

        public void Init(Color enableColor)
        {
            if (targetImage != null) drawType = DRAWTYPE.IMAGE;
            else if (targetSprite != null) drawType = DRAWTYPE.SPRITE;
            else if (meshRender != null) drawType = DRAWTYPE.MESHRENDER;
            else if (textMesh != null)
            {
                drawType = DRAWTYPE.TEXTMESH;
                EnableColor = enableColor;
                DisableColor = Constants.btnColor_Dis;
            }

            materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetColor(SWITCHSTATE state, bool isTouch)
        {
            Color _color = DisableColor;

            if (isTouch)
            {
                _color = TouchColor;
            }
            else
            {
                switch (state)
                {
                    case SWITCHSTATE.OFF:
                        _color = DisableColor;
                        break;
                    case SWITCHSTATE.ON:
                        _color = EnableColor;
                        break;
                }
            }


            switch (drawType)
            {
                case DRAWTYPE.IMAGE:
                    targetImage.color = _color;
                    break;
                case DRAWTYPE.SPRITE:
                    targetSprite.color = _color;
                    break;
                case DRAWTYPE.MESHRENDER:
                    materialPropertyBlock.SetColor("_Color", _color);
                    meshRender.SetPropertyBlock(materialPropertyBlock);
                    break;
                case DRAWTYPE.TEXTMESH:
                    textMesh.color = _color;
                    break;
            }
        }
    }
}