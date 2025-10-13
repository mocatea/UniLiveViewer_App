using VContainer;

namespace UniLiveViewer.Stage.Candy
{
    public class CandyEnvironmentService
    {
        readonly CandyEnvironmentSettings _settings;

        [Inject]
        public CandyEnvironmentService(CandyEnvironmentSettings settings)
        {
            _settings = settings;
        }

        public void OnClickParticle(bool isEnable)
        {
            _settings.Particle.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_crs_particle = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickLaserGun(bool isEnable)
        {
            _settings.LaserGun.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_crs_laser = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickReflection(bool isEnable)
        {
            _settings.FloorMirror.material.SetFloat(_settings.SmoothnessId, isEnable == true ? 1 : 0);
            FileReadAndWriteUtility.UserProfile.scene_crs_reflection = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickSonicBoom(bool isEnable)
        {
            _settings.SonicBoom.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_crs_sonic = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickPlayManual(bool isEnable)
        {
            _settings.ManualUI.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_crs_manual = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }
    }
}