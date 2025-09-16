using System;
using UnityEngine;

namespace UniLiveViewer.SO
{
    [CreateAssetMenu(menuName = "MyGame/SceneInitialData", fileName = "InitialData_")]
    public class SceneInitialData : ScriptableObject
    {
        public LightData Light => _light;
        [SerializeField] LightData _light;

        public bool Tonemapping => _tonemapping;
        [SerializeField] bool _tonemapping;

        public BloomInitialData Bloom => _bloom;
        [SerializeField] BloomInitialData _bloom;
    }

    [Serializable]
    public class LightData
    {
        public float Intensity => _intensity;
        [SerializeField] float _intensity = 1f;

        public Color Color => _color;
        [SerializeField] Color _color = Color.white;
    }

    [Serializable]
    public class BloomInitialData
    {
        public bool UseBloom => _useBloom;
        [SerializeField] bool _useBloom;

        public float Threshold => _threshold;
        [SerializeField] float _threshold = 0.95f;

        public float Intensity => _intensity;
        [SerializeField] float _intensity = 3f;

        public float Scatter => _scatter;
        [SerializeField] float _scatter = 0.5f;

        public bool UseColor => _useColor;
        [SerializeField] bool _useColor;

        public float Hue => _hue;
        [SerializeField] float _hue = 0.65f;//水色
    }
}

