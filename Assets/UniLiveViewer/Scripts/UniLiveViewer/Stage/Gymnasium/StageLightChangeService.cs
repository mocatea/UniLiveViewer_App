using VContainer;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class StageLightChangeService
    {
        bool _isWhite;
        int _currntIndex;
        int _actorCount;

        readonly GymnasiumEnvironmentSettings _settings;

        [Inject]
        public StageLightChangeService(GymnasiumEnvironmentSettings settings)
        {
            _settings = settings;

            _isWhite = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;
            _currntIndex = StageEnums.StageLightDefaultIndex;
        }

        public void OnChangeStageLight(int index)
        {
            _currntIndex = index;
            UpdateStageLight();

            //各要素反映
            OnChangeSummonedCount(_actorCount);
            OnChangeLightColor(_isWhite);
        }

        /// <summary>
        /// ライトの種類をCurrentに切り替える
        /// </summary>
        void UpdateStageLight()
        {
            for (int i = 0; i < _settings.Lights.Length; i++)
            {
                _settings.Lights[i].gameObject.SetActive(i == _currntIndex);
            }
        }

        /// <summary>
        /// 召喚数更新時
        /// </summary>
        public void OnChangeSummonedCount(int actorCount)
        {
            _actorCount = actorCount;
            if (_settings.Stagelights.Length <= _currntIndex) return;
            _settings.Stagelights[_currntIndex].ChangeCount(actorCount);
        }

        /// <summary>
        /// ライトカラー更新時
        /// （UI開いた時にも通知きてる）
        /// </summary>
        public void OnChangeLightColor(bool isWhite)
        {
            _isWhite = isWhite;
            if (_settings.Stagelights.Length <= _currntIndex) return;
            _settings.Stagelights[_currntIndex].ChangeColor(isWhite);
        }

        public void OnTick()
        {
            if (_settings.Stagelights.Length <= _currntIndex) return;
            _settings.Stagelights[_currntIndex].OnUpdate();
        }
    }
}