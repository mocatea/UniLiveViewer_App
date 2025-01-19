using System.Linq;
using UnityEngine;

namespace UniLiveViewer.Stage.Gymnasium
{
    // 作りイマイチ
    public class StageLightChangeService : MonoBehaviour
    {
        [SerializeField] Transform[] _lights = new Transform[5];
        bool _isWhite;
        int _currnt;
        IStageLight[] _stagelights;
        int _charaCount;

        void Awake()
        {
            _isWhite = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;
            _currnt = StageEnums.StageLightDefaultIndex;
            UpdateStageLight();

            _stagelights = _lights
                .Select(t => t.GetComponent<IStageLight>())
                .Where(stageLight => stageLight != null)
                .ToArray();
        }

        /// <summary>
        /// ライトの種類をCurrentに切り替える
        /// </summary>
        void UpdateStageLight()
        {
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].gameObject.SetActive(i == _currnt);
            }
        }

        public void OnChangeStageLight(int index)
        {
            _currnt = index;
            UpdateStageLight();

            //各要素反映
            OnChangeSummonedCount(_charaCount);
            OnChangeLightColor(_isWhite);
        }

        /// <summary>
        /// 召喚数更新時
        /// </summary>
        public void OnChangeSummonedCount(int count)
        {
            _charaCount = count;
            if (_stagelights.Length <= _currnt) return;
            _stagelights[_currnt].ChangeCount(count);
        }

        /// <summary>
        /// ライトカラー更新時
        /// （UI開いた時にも通知きてる）
        /// </summary>
        public void OnChangeLightColor(bool isWhite)
        {
            _isWhite = isWhite;
            if (_stagelights.Length <= _currnt) return;
            _stagelights[_currnt].ChangeColor(isWhite);
        }

        public void OnTick()
        {
            if (_stagelights.Length <= _currnt) return;
            _stagelights[_currnt].OnUpdate();
        }
    }
}