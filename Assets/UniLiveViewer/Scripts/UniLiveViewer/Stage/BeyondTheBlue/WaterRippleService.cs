using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    /// <summary>
    /// 水面に波紋を発火するクラス
    /// </summary>
    public class WaterRippleService
    {
        readonly int[] BankA = {
            Shader.PropertyToID("_Ripple0"),
            Shader.PropertyToID("_Ripple1"),
            Shader.PropertyToID("_Ripple2"),
        };
        readonly int[] BankB = {
            Shader.PropertyToID("_Ripple3"),
            Shader.PropertyToID("_Ripple4"),
            Shader.PropertyToID("_Ripple5"),
        };
        readonly int[] BankC = {
            Shader.PropertyToID("_Ripple6"),
            Shader.PropertyToID("_Ripple7"),
            Shader.PropertyToID("_Ripple8"),
        };
        readonly int[] BankD = {
            Shader.PropertyToID("_Ripple9"),
            Shader.PropertyToID("_Ripple10"),
            Shader.PropertyToID("_Ripple11"),
        };

        Dictionary<int, int[]> _map;
        Renderer _renderer;
        int _currentBank = 0;

        /// <summary> (x,z,start,amp) </summary>
        readonly Vector4[] _slots = new Vector4[3];
        readonly MaterialPropertyBlock _mpb = new();
        readonly WaterRippleSettings _settings;

        [Inject]
        public WaterRippleService(WaterRippleSettings settings)
        {
            _settings = settings;
            _renderer = _settings.WaterRenderer;

            _map = new Dictionary<int, int[]>()
            {
                {0,BankA },{1,BankB },{2,BankC },{3,BankD }
            };
        }

        /// <summary>
        /// Y成分は無視される
        /// </summary>
        public void PushRipple(Vector3 pos)
        {
            _renderer.GetPropertyBlock(_mpb);
            ClearRipples(_currentBank);

            var local = _renderer.transform.InverseTransformPoint(pos);
            _slots[0] = new Vector4(local.x, local.z, Time.time, _settings.WaterRippleAmp);
            _slots[1] = new Vector4(local.x, local.z, Time.time + _settings.WaterRippleInterval, _settings.WaterRippleAmp);
            _slots[2] = new Vector4(local.x, local.z, Time.time + (_settings.WaterRippleInterval * 2), _settings.WaterRippleAmp);
            Ripple(_currentBank, _slots);

            _renderer.SetPropertyBlock(_mpb);

            _currentBank++;
            if (_currentBank >= 4) _currentBank = 0;
        }

        void ClearRipples(int bank)
        {
            var ids = _map[bank];
            _mpb.SetVector(ids[0], Vector4.zero);
            _mpb.SetVector(ids[1], Vector4.zero);
            _mpb.SetVector(ids[2], Vector4.zero);
        }

        void Ripple(int bank, Vector4[] pos)
        {
            var ids = _map[bank];
            _mpb.SetVector(ids[0], pos[0]);
            _mpb.SetVector(ids[1], pos[1]);
            _mpb.SetVector(ids[2], pos[2]);
        }
    }
}