using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class CandyLiveMenuServie : IStageMenuService
    {
        Transform[] _actionObj = new Transform[5];
        Material _matMirrore;

        readonly CandyLiveMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;

        [Inject]
        public CandyLiveMenuServie(CandyLiveMenuSettings settings,RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.ParticleButton.onTrigger += (btn) => OnClickParticle(btn.isEnable);
            _settings.LaserGunButton.onTrigger += (btn) => OnClickLaserGun(btn.isEnable);
            _settings.ReflectionButton.onTrigger += (btn) => OnClickReflection(btn.isEnable);
            _settings.SonicBoomButton.onTrigger += (btn) => OnClickSonicBoom(btn.isEnable);
            _settings.PlayManualButton.onTrigger += (btn) => OnClickPlayManual(btn.isEnable);

            _actionObj[0] = GameObject.FindGameObjectWithTag("Particle").transform;
            _actionObj[1] = GameObject.FindGameObjectWithTag("LaserGun").transform;
            _actionObj[2] = GameObject.FindGameObjectWithTag("FloorMirror").transform;
            _matMirrore = _actionObj[2].GetComponent<MeshRenderer>().material;
            _actionObj[3] = GameObject.FindGameObjectWithTag("SonicBoom").transform;
            _actionObj[4] = GameObject.FindGameObjectWithTag("ManualUI").transform;

        }

        void IStageMenuService.OnEnable()
        {
            _actionObj[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_particle);
            _actionObj[1].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_laser);
            _matMirrore.SetFloat("_Smoothness", FileReadAndWriteUtility.UserProfile.scene_crs_reflection ? 1 : 0);
            _actionObj[3].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_sonic);
            _actionObj[4].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_manual);

            _settings.ParticleButton.isEnable = _actionObj[0].gameObject.activeSelf;
            _settings.LaserGunButton.isEnable = _actionObj[1].gameObject.activeSelf;
            _settings.ReflectionButton.isEnable = (_matMirrore.GetFloat("_Smoothness") == 1.0f);
            _settings.SonicBoomButton.isEnable = _actionObj[3].gameObject.activeSelf;
            _settings.PlayManualButton.isEnable = _actionObj[4].gameObject.activeSelf;
        }

        void OnClickParticle(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[0])
            {
                _actionObj[0].gameObject.SetActive(isEnable);
                FileReadAndWriteUtility.UserProfile.scene_crs_particle = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void OnClickLaserGun(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[1])
            {
                _actionObj[1].gameObject.SetActive(isEnable);
                FileReadAndWriteUtility.UserProfile.scene_crs_laser = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void OnClickReflection(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_matMirrore)
            {
                _matMirrore.SetFloat("_Smoothness", isEnable == true ? 1 : 0);
                FileReadAndWriteUtility.UserProfile.scene_crs_reflection = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void OnClickSonicBoom(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[3])
            {
                _actionObj[3].gameObject.SetActive(isEnable);
                FileReadAndWriteUtility.UserProfile.scene_crs_sonic = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void OnClickPlayManual(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[4])
            {
                _actionObj[4].gameObject.SetActive(isEnable);
                FileReadAndWriteUtility.UserProfile.scene_crs_manual = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void IStageMenuService.Dispose()
        {
            _settings.ParticleButton.onTrigger -= (btn) => OnClickParticle(btn.isEnable);
            _settings.LaserGunButton.onTrigger -= (btn) => OnClickLaserGun(btn.isEnable);
            _settings.ReflectionButton.onTrigger -= (btn) => OnClickReflection(btn.isEnable);
            _settings.SonicBoomButton.onTrigger -= (btn) => OnClickSonicBoom(btn.isEnable);
            _settings.PlayManualButton.onTrigger -= (btn) => OnClickPlayManual(btn.isEnable);
        }
    }
}