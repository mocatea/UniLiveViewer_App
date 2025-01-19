using System;
using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class KaguraLiveMenuServie : IStageMenuService
    {
        public IObservable<bool> IsParticleAsObservable => _isParticle;
        readonly Subject<bool> _isParticle = new();

        public IObservable<bool> IsReflectionAsObservable => _isReflection;
        readonly Subject<bool> _isReflection = new();

        public IObservable<bool> IsSeaWavesAsObservable => _isSeaWaves;
        readonly Subject<bool> _isSeaWaves = new();

        public IReactiveProperty<float> FogDensity => _fogDensity;
        readonly ReactiveProperty<float> _fogDensity = new();


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
                .Subscribe(x => _fogDensity.Value = x).AddTo(_disposables);
            _settings.FogSlider.Value = 0.02f;
        }

        void IStageMenuService.OnEnable()
        {
            _settings.ParticleButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_kagura_particle;
            _settings.ReflectionButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_kagura_sea;
            _settings.SeaWavesButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_kagura_reflection;
        }

        void OnClickParticle(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isParticle.OnNext(isEnable);
        }

        void OnClickReflection(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isReflection.OnNext(isEnable);
        }

        void OnClickSeaWaves(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isSeaWaves.OnNext(isEnable);
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