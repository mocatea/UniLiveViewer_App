using System;
using UnityEngine;

namespace UniLiveViewer.Timeline
{
    public enum SHADOWTYPE
    {
        NONE,
        CIRCLE,
        CROSS,
        NONE_CIRCLE,
        NONE_CROSS,
        CIRCLE_CIRCLE,
        CROSS_CROSS,
        CIRCLE_CROSS,
        CROSS_CIRCLE,
    }

    // SerializeField沢山
    public class QuasiShadowSetting : MonoBehaviour
    {
        public SHADOWTYPE ShadowType
        {
            get { return _shadowType; }
            set
            {
                _shadowType = value;
                if ((int)_shadowType >= _typeLength) _shadowType = 0;
                else if ((int)_shadowType < 0) _shadowType = (SHADOWTYPE)(_typeLength - 1);
            }
        }
        [Header("確認用露出(readonly)")]
        [SerializeField] SHADOWTYPE _shadowType = SHADOWTYPE.NONE;
        int _typeLength = Enum.GetNames(typeof(SHADOWTYPE)).Length;

        public MeshRenderer MeshRendererPrefab => _meshRendererPrefab;
        [SerializeField] MeshRenderer _meshRendererPrefab;

        public float ShadowScale => _shadowScale;
        [SerializeField] float _shadowScale = 1.0f;

        public float FootRay => _footRay;
        [SerializeField] float _footRay = 0.05f;

        /// <summary>
        /// 腰上げの影減衰倍率(≒値を上げる程影が小さくなりやすい)
        /// </summary>
        public float BodyAttenuationMultiplier => _bodyAttenuationMultiplier;
        [SerializeField] float _bodyAttenuationMultiplier = 0.4f;

        /// <summary>
        /// 足上げの影減衰倍率(≒値を上げる程影が小さくなりやすい)
        /// </summary>
        public float FootAttenuationMultiplier => _footAttenuationMultiplier;
        [SerializeField]  float _footAttenuationMultiplier = 1.5f;

        public Preset[] Presets => _preset;
        [SerializeField] Preset[] _preset;

        // TODO: 雑なので改善する
        [Serializable]
        public class Preset
        {
            public SHADOWTYPE shadowType;
            public Texture2D texture_Body;
            public Texture2D texture_Foot;
            public float scala_Body;
            public float scala_Foot;
        }

        public void SetShadowScale(float value)
        {
            _shadowScale = value;
        }
    }
}
