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

        [Inject]
        public ViewerMenuServie(ViewerMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            _settings.LedButton.onTrigger += (btn) => OnClickFloorLED(btn.isEnable);
            _backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();

            for (int i = 0; i < _settings.ParticleButtons.Length; i++)
            {
                _settings.ParticleButtons[i].onTrigger += (btn) =>
                {
                    // これやらんと発火時はLengthの値になる
                    var moveIndex = btn == _settings.ParticleButtons[0] ? -1 : 1;
                    OnClickParticle(moveIndex);
                };
            }
            for (int i = 0; i < _settings.WormHolleButtons.Length; i++)
            {
                _settings.WormHolleButtons[i].onTrigger += (btn) =>
                {
                    var moveIndex = btn == _settings.WormHolleButtons[0] ? -1 : 1;
                    OnClickWormHolle(moveIndex);
                };
            }
            for (int i = 0; i < _settings.SkyBoxButtons.Length; i++)
            {
                _settings.SkyBoxButtons[i].onTrigger += (btn) =>
                {
                    var moveIndex = btn == _settings.SkyBoxButtons[0] ? -1 : 1;
                    OnClickSkyBox(moveIndex);
                };
            }
        }

        void IStageMenuService.OnEnable()
        {
            _settings.LedButton.isEnable = FileReadAndWriteUtility.UserProfile.scene_view_led;
        }

        void OnClickParticle(int moveIndex)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _settings.Texts[0].text = "Particle_" + _backGroundCon.GetParticleName(moveIndex);
            _particleMoveIndex.OnNext(moveIndex);
        }

        void OnClickWormHolle(int moveIndex)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _settings.Texts[1].text = "WormHolle_" + _backGroundCon.GetWormHolleName(moveIndex);
            _wormHolleMoveIndex.OnNext(moveIndex);
        }

        void OnClickSkyBox(int moveIndex)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _settings.Texts[2].text = "SkyBox_" + _backGroundCon.GetCubemapName(moveIndex);
            _skyboxMoveIndex.OnNext(moveIndex);
        }

        void OnClickFloorLED(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            _isFloorLED.OnNext(isEnable);
        }

        void IStageMenuService.Dispose()
        {
            _settings.LedButton.onTrigger -= (btn) => OnClickFloorLED(btn.isEnable);

            for (int i = 0; i < _settings.ParticleButtons.Length; i++)
            {
                _settings.ParticleButtons[i].onTrigger -= (btn) => OnClickParticle(0);
            }
            for (int i = 0; i < _settings.WormHolleButtons.Length; i++)
            {
                _settings.WormHolleButtons[i].onTrigger -= (btn) => OnClickWormHolle(0);
            }
            for (int i = 0; i < _settings.SkyBoxButtons.Length; i++)
            {
                _settings.SkyBoxButtons[i].onTrigger -= (btn) => OnClickSkyBox(0);
            }
        }
    }
}