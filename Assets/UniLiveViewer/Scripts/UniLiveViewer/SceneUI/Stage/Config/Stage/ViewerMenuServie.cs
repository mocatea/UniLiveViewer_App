using System;
using UniLiveViewer.Stage;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class ViewerMenuServie : IStageMenuService
    {
        public IObservable<int> ParticleMoveIndexAsObservable => _particleMoveIndex;
        readonly Subject<int> _particleMoveIndex = new();

        public IObservable<int> WormHolleMoveIndexAsObservable => _wormHolleMoveIndex;
        readonly Subject<int> _wormHolleMoveIndex = new();

        public IObservable<int> SkyboxMoveIndexAsObservable => _skyboxMoveIndex;
        readonly Subject<int> _skyboxMoveIndex = new();

        public IObservable<bool> IsFloorLEDAsObservable => _isFloorLED;
        readonly Subject<bool> _isFloorLED = new();

        BackGroundController _backGroundCon;

        readonly ViewerMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public ViewerMenuServie(ViewerMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.LedButton.OnTriggerAsObservable()
                    .Subscribe(x => OnClickFloorLED(x.isEnable)).AddTo(_disposables);

            _backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();


            _settings.ParticleButtons[0].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickParticle(-1)).AddTo(_disposables);
            _settings.ParticleButtons[1].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickParticle(1)).AddTo(_disposables);

            _settings.WormHolleButtons[0].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickWormHolle(-1)).AddTo(_disposables);
            _settings.WormHolleButtons[1].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickWormHolle(1)).AddTo(_disposables);

            _settings.SkyBoxButtons[0].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickSkyBox(-1)).AddTo(_disposables);
            _settings.SkyBoxButtons[1].OnTriggerAsObservable()
                    .Subscribe(_ => OnClickSkyBox(1)).AddTo(_disposables);
        }

        void IStageMenuService.OnEnable()
        {
            _settings.LedButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_view_led;
        }

        void OnClickParticle(int moveIndex)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _settings.Texts[0].text = _backGroundCon.GetParticleName(moveIndex);
            _particleMoveIndex.OnNext(moveIndex);
        }

        void OnClickWormHolle(int moveIndex)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _settings.Texts[1].text = _backGroundCon.GetWormHolleName(moveIndex);
            _wormHolleMoveIndex.OnNext(moveIndex);
        }

        void OnClickSkyBox(int moveIndex)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _settings.Texts[2].text = _backGroundCon.GetCubemapName(moveIndex);
            _skyboxMoveIndex.OnNext(moveIndex);
        }

        void OnClickFloorLED(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isFloorLED.OnNext(isEnable);
        }

        void IStageMenuService.Dispose()
        {
            _disposables.Dispose();
        }
    }
}