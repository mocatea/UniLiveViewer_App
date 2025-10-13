using VContainer;

namespace UniLiveViewer.Stage.Viewer
{
    public class ViewerEnvironmentService
    {
        readonly ViewerEnvironmentSettings _settings;

        [Inject]
        public ViewerEnvironmentService(ViewerEnvironmentSettings settings)
        {
            _settings = settings;
        }

        public void OnClickParticle(int moveIndex)
        {
            _settings.BackGroundCon.SetParticle(moveIndex);
        }

        public void OnClickWormHole(int moveIndex)
        {
            _settings.BackGroundCon.SetWormHole(moveIndex);
        }

        public void OnClickSkyBox(int moveIndex)
        {
            _settings.BackGroundCon.SetCubemap(moveIndex);
        }

        public void OnClickFloorLED(bool isEnable)
        {
            _settings.FloorLED.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_view_led = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }
    }
}