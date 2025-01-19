using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class KaguraLiveMenuServie : IStageMenuService
    {
        Transform[] _actionObj = new Transform[3];

        readonly KaguraLiveMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public KaguraLiveMenuServie(KaguraLiveMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.ParticleButton.onTrigger += (btn) => OnClickParticle(btn.isEnable);
            _settings.ReflectionButton.onTrigger += (btn) => OnClickReflection(btn.isEnable);
            _settings.SeaWavesButton.onTrigger += (btn) => OnClickSeaWaves(btn.isEnable);
            
            _settings.FogSlider.ValueAsObservable
                .Subscribe(x => RenderSettings.fogDensity = x).AddTo(_disposables);
            _settings.FogSlider.Value = 0.02f;

            _actionObj[0] = GameObject.FindGameObjectWithTag("Particle").transform;
            _actionObj[1] = GameObject.FindGameObjectWithTag("ReflectionProbe").transform;
            _actionObj[2] = GameObject.FindGameObjectWithTag("WaterAnchor").transform;
        }

        void IStageMenuService.OnEnable()
        {
            _actionObj[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_particle);
            _actionObj[1].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_sea);
            _actionObj[2].transform.GetChild(0).gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_reflection);

            _settings.ParticleButton.isEnable = _actionObj[0].gameObject.activeSelf;
            _settings.ReflectionButton.isEnable = _actionObj[1].gameObject.activeSelf;
            _settings.SeaWavesButton.isEnable = _actionObj[2].transform.GetChild(0).gameObject.activeSelf;
        }

        void OnClickParticle(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[0])
            {
                _actionObj[0].gameObject.SetActive(isEnable);
                FileReadAndWriteUtility.UserProfile.scene_kagura_particle = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void OnClickReflection(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[1])
            {
                _actionObj[1].gameObject.SetActive(isEnable);
                FileReadAndWriteUtility.UserProfile.scene_kagura_reflection = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void OnClickSeaWaves(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[2])
            {
                if (_actionObj[2].GetChild(0).gameObject.activeSelf)
                {
                    _actionObj[2].GetChild(0).gameObject.SetActive(false);
                    _actionObj[2].GetChild(1).gameObject.SetActive(true);
                }
                else if (_actionObj[2].GetChild(1).gameObject.activeSelf)
                {
                    _actionObj[2].GetChild(1).gameObject.SetActive(false);
                    _actionObj[2].GetChild(0).gameObject.SetActive(true);
                }
                FileReadAndWriteUtility.UserProfile.scene_kagura_sea = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void IStageMenuService.Dispose()
        {
            _settings.ParticleButton.onTrigger -= (btn) => OnClickParticle(btn.isEnable);
            _settings.ReflectionButton.onTrigger -= (btn) => OnClickReflection(btn.isEnable);
            _settings.SeaWavesButton.onTrigger -= (btn) => OnClickSeaWaves(btn.isEnable);
            _disposables.Dispose();
        }
    }
}