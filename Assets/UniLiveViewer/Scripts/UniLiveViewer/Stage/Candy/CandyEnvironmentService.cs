using UnityEngine;

namespace UniLiveViewer.Stage.Candy
{
    public class CandyEnvironmentService : MonoBehaviour
    {
        Transform _particle;
        Transform _laserGun;
        Material _matMirrore;
        Transform _sonicBoom;
        Transform _manualUI;

        void Start()
        {
            _particle = GameObject.FindGameObjectWithTag("Particle").transform;
            _laserGun = GameObject.FindGameObjectWithTag("LaserGun").transform;
            var floorMirror = GameObject.FindGameObjectWithTag("FloorMirror").transform;
            _matMirrore = floorMirror.GetComponent<MeshRenderer>().material;
            _sonicBoom = GameObject.FindGameObjectWithTag("SonicBoom").transform;
            _manualUI = GameObject.FindGameObjectWithTag("ManualUI").transform;

            _particle.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_particle);
            _laserGun.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_laser);
            _matMirrore.SetFloat("_Smoothness", FileReadAndWriteUtility.UserProfile.scene_crs_reflection ? 1 : 0);
            _sonicBoom.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_sonic);
            _manualUI.gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_manual);
        }

        public void OnClickParticle(bool isEnable)
        {
            if (_particle == null) return;

            _particle.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_crs_particle = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickLaserGun(bool isEnable)
        {
            if (_laserGun == null) return;

            _laserGun.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_crs_laser = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickReflection(bool isEnable)
        {
            if (_matMirrore == null) return;

            _matMirrore.SetFloat("_Smoothness", isEnable == true ? 1 : 0);
            FileReadAndWriteUtility.UserProfile.scene_crs_reflection = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickSonicBoom(bool isEnable)
        {
            if (_sonicBoom == null) return;

            _sonicBoom.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_crs_sonic = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void OnClickPlayManual(bool isEnable)
        {
            if (_manualUI == null) return;

            _manualUI.gameObject.SetActive(isEnable);
            FileReadAndWriteUtility.UserProfile.scene_crs_manual = isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }
    }
}