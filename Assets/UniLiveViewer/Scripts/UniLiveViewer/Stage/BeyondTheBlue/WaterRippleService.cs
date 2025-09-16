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
        readonly int[] Bank = {
            Shader.PropertyToID("_Ripple0"),
            Shader.PropertyToID("_Ripple1"),
            Shader.PropertyToID("_Ripple2"),
            Shader.PropertyToID("_Ripple3"),
            Shader.PropertyToID("_Ripple4"),
            Shader.PropertyToID("_Ripple5"),
            Shader.PropertyToID("_Ripple6"),
            Shader.PropertyToID("_Ripple7"),
            Shader.PropertyToID("_Ripple8"),
            Shader.PropertyToID("_Ripple9"),
            Shader.PropertyToID("_Ripple10"),
            Shader.PropertyToID("_Ripple11"),
        };

        readonly int[] BankA = {
            Shader.PropertyToID("_Ripple0"),
            Shader.PropertyToID("_Ripple1"),
        };
        readonly int[] BankB = {
            Shader.PropertyToID("_Ripple2"),
            Shader.PropertyToID("_Ripple3"),

        };
        readonly int[] BankC = {
            Shader.PropertyToID("_Ripple4"),
            Shader.PropertyToID("_Ripple5"),

        };
        readonly int[] BankD = {
            Shader.PropertyToID("_Ripple6"),
            Shader.PropertyToID("_Ripple7"),
        };
        readonly int[] BankE = {
            Shader.PropertyToID("_Ripple8"),
            Shader.PropertyToID("_Ripple9"),
        };
        readonly int[] BankF = {
            Shader.PropertyToID("_Ripple10"),
            Shader.PropertyToID("_Ripple11"),
        };

        Dictionary<int, int[]> _map;
        Renderer _renderer;
        int _currentBank = 0;
        bool _marutiRipple = true;

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
                {0,BankA },{1,BankB },{2,BankC },{3,BankD },{4,BankE },{5,BankF }
            };
        }

        /// <summary>
        /// Y成分は無視される
        /// </summary>
        public void PushRipple(Vector3 pos)
        {
            var local = _renderer.transform.InverseTransformPoint(pos);
            _renderer.GetPropertyBlock(_mpb);
            if (_marutiRipple)
            {
                MarutiRipple(local);
            }
            else
            {
                Ripple(local);
            }
            _renderer.SetPropertyBlock(_mpb);
        }

        void MarutiRipple(Vector3 localPos)
        {
            var ids = _map[_currentBank];
            _mpb.SetVector(ids[0], Vector4.zero);
            _mpb.SetVector(ids[1], Vector4.zero);

            _slots[0] = new Vector4(localPos.x, localPos.z, Time.time, _settings.WaterRippleAmp);
            _slots[1] = new Vector4(localPos.x, localPos.z, Time.time + _settings.WaterRippleInterval, _settings.WaterRippleAmp);
            //_slots[2] = new Vector4(local.x, local.z, Time.time + (_settings.WaterRippleInterval * 2), _settings.WaterRippleAmp);
            
            _mpb.SetVector(ids[0], _slots[0]);
            _mpb.SetVector(ids[1], _slots[1]);

            _currentBank++;
            if (_currentBank >= _map.Count) _currentBank = 0;
        }

        void Ripple(Vector3 localPos)
        {
            var id = Bank[_currentBank];
            _mpb.SetVector(id, Vector4.zero);
            _slots[0] = new Vector4(localPos.x, localPos.z, Time.time, _settings.WaterRippleAmp);
            _mpb.SetVector(id, _slots[0]);

            _currentBank++;
            if (_currentBank >= Bank.Length) _currentBank = 0;
        }
    }
}