using UnityEngine;
using VContainer;

namespace UniLiveViewer.Stage.Kagura
{
    public class KaguraEnvironmentService
    {
        readonly KaguraEnvironmentSettings _sttings;

        [Inject]
        public KaguraEnvironmentService(KaguraEnvironmentSettings sttings)
        {
            _sttings = sttings;
        }

        public void OnClickParticle(bool isEnable)
        {
            _sttings.Particle.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_kagura_particle = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickReflection(bool isEnable)
        {
            _sttings.Reflection.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_kagura_reflection = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickSeaWaves(bool isEnable)
        {
            // 反転で切り替え
            if (_sttings.WaterAnchor.GetChild(0).gameObject.activeSelf)
            {
                _sttings.WaterAnchor.GetChild(0).gameObject.SetActive(false);
                _sttings.WaterAnchor.GetChild(1).gameObject.SetActive(true);
            }
            else if (_sttings.WaterAnchor.GetChild(1).gameObject.activeSelf)
            {
                _sttings.WaterAnchor.GetChild(1).gameObject.SetActive(false);
                _sttings.WaterAnchor.GetChild(0).gameObject.SetActive(true);
            }

            _sttings.WaterAnchor.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_kagura_sea = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnChangeFog(float v)
        {
            RenderSettings.fogDensity = v;
        }
    }
}