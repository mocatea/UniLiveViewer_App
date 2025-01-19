using UnityEngine;

namespace UniLiveViewer.Stage.Kagura
{
    public class KaguraEnvironmentService : MonoBehaviour
    {
        Transform _particle;
        Transform _reflection;
        Transform _waterAnchor;

        void Start()
        {
            _particle = GameObject.FindGameObjectWithTag("Particle").transform;
            _reflection = GameObject.FindGameObjectWithTag("ReflectionProbe").transform;
            _waterAnchor = GameObject.FindGameObjectWithTag("WaterAnchor").transform;

            _particle.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_particle);
            _reflection.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_sea);
            _waterAnchor.transform.GetChild(0).gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_reflection);
        }

        public void OnClickParticle(bool isEnable)
        {
            if (_particle == null) return;

            _particle.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_kagura_particle = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickReflection(bool isEnable)
        {
            if (_reflection == null) return;

            _reflection.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_kagura_reflection = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickSeaWaves(bool isEnable)
        {
            if (_waterAnchor == null) return;

            // 反転で切り替え
            if (_waterAnchor.GetChild(0).gameObject.activeSelf)
            {
                _waterAnchor.GetChild(0).gameObject.SetActive(false);
                _waterAnchor.GetChild(1).gameObject.SetActive(true);
            }
            else if (_waterAnchor.GetChild(1).gameObject.activeSelf)
            {
                _waterAnchor.GetChild(1).gameObject.SetActive(false);
                _waterAnchor.GetChild(0).gameObject.SetActive(true);
            }

            _waterAnchor.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_kagura_sea = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickFog(float v)
        {
            RenderSettings.fogDensity = v;
        }
    }
}