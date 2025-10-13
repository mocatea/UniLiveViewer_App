

using UnityEngine;

namespace UniLiveViewer.MessagePipe
{
    /// <summary>
    /// 海ステの水波紋用
    /// </summary>
    public class WaterRippleMessage
    {
        public Vector3 Position => _pos;
        Vector3 _pos;

        public WaterRippleMessage(Vector3 pos)
        {
            _pos = pos;
        }
    }
}