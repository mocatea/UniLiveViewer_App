using System;
using UniRx;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class CandyLiveMenuServie : IStageMenuService
    {
        public IObservable<bool> IsParticleAsObservable => _isParticle;
        readonly Subject<bool> _isParticle = new();

        public IObservable<bool> IsLaserGunAsObservable => _isLaserGun;
        readonly Subject<bool> _isLaserGun = new();

        public IObservable<bool> IsReflectionAsObservable => _isReflection;
        readonly Subject<bool> _isReflection = new();

        public IObservable<bool> IsSonicBoomAsObservable => _isSonicBoom;
        readonly Subject<bool> _isSonicBoom = new();

        public IObservable<bool> IsPlayManualAsObservable => _isPlayManual;
        readonly Subject<bool> _isPlayManual = new();

        readonly CandyLiveMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public CandyLiveMenuServie(CandyLiveMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.ParticleButton.OnTriggerAsObservable()
                .Subscribe(x => OnClickParticle(x.isEnable)).AddTo(_disposables);
            _settings.LaserGunButton.OnTriggerAsObservable()
                .Subscribe(x => OnClickLaserGun(x.isEnable)).AddTo(_disposables);
            _settings.ReflectionButton.OnTriggerAsObservable()
                .Subscribe(x => OnClickReflection(x.isEnable)).AddTo(_disposables);
            _settings.SonicBoomButton.OnTriggerAsObservable()
                .Subscribe(x => OnClickSonicBoom(x.isEnable)).AddTo(_disposables);
            _settings.PlayManualButton.OnTriggerAsObservable()
                .Subscribe(x => OnClickPlayManual(x.isEnable)).AddTo(_disposables);
        }

        void IStageMenuService.OnEnable()
        {
            _settings.ParticleButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_crs_particle;
            _settings.LaserGunButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_crs_laser;
            _settings.ReflectionButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_crs_reflection;
            _settings.SonicBoomButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_crs_sonic;
            _settings.PlayManualButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_crs_manual;
        }

        void OnClickParticle(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isParticle.OnNext(isEnable);
        }

        void OnClickLaserGun(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isLaserGun.OnNext(isEnable);
        }

        void OnClickReflection(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isReflection.OnNext(isEnable);
        }

        void OnClickSonicBoom(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isSonicBoom.OnNext(isEnable);
        }

        void OnClickPlayManual(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isPlayManual.OnNext(isEnable);
        }

        void IStageMenuService.Dispose()
        {
            _disposables.Dispose();
        }
    }
}