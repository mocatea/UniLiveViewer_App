using UnityEngine;
using VContainer;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class GymnasiumEnvironmentService
    {
        readonly StageLightChangeService _stageLightChangeService;

        [Inject]
        public GymnasiumEnvironmentService(StageLightChangeService stageLightChangeService)
        {
            _stageLightChangeService = stageLightChangeService;
        }

        public void OnChangeSummonedCount(int count)
        {
            _stageLightChangeService.OnChangeSummonedCount(count);
        }

        public void OnChangeStageLight(int index)
        {
            _stageLightChangeService.OnChangeStageLight(index);
        }

        public void OnClickWhiteLightColor(bool isWhite)
        {
            _stageLightChangeService.OnChangeLightColor(isWhite);
            FileReadAndWriteUtility.UserProfile.scene_gym_whitelight = isWhite;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnTick()
        {
            _stageLightChangeService.OnTick();
        }
    }
}