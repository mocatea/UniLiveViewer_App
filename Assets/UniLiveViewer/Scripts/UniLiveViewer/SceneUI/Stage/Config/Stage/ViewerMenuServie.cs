using UniLiveViewer.Stage;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class ViewerMenuServie : IStageMenuService
    {
        Transform[] _actionObj = new Transform[1];
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
            _settings.LedButton.onTrigger += (btn) => OnClick(0, btn.isEnable);

            for (int i = 0; i < _settings.ParticleButtons.Length; i++)
            {
                _settings.ParticleButtons[i].onTrigger += (btn) =>
                {
                    // これやらんと発火時はLengthの値になる
                    var index = btn == _settings.ParticleButtons[0] ? 0 : 1;
                    OnClickParticle(index);
                };
            }
            for (int i = 0; i < _settings.WormHolleButtons.Length; i++)
            {
                _settings.WormHolleButtons[i].onTrigger += (btn) =>
                {
                    var index = btn == _settings.WormHolleButtons[0] ? 0 : 1;
                    OnClickWormHolle(index);
                };
            }
            for (int i = 0; i < _settings.SkyBoxButtons.Length; i++)
            {
                _settings.SkyBoxButtons[i].onTrigger += (btn) =>
                {
                    var index = btn == _settings.SkyBoxButtons[0] ? 0 : 1;
                    OnClickSkyBox(index);
                };
            }

            _actionObj[0] = GameObject.FindGameObjectWithTag("FloorLED").transform;
            _backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();
        }

        void IStageMenuService.OnEnable()
        {
            _actionObj[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_view_led);
            _settings.LedButton.isEnable = _actionObj[0].gameObject.activeSelf;
        }

        void OnClick(int index, bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            switch (index)
            {
                //LED
                case 0:
                    if (_actionObj[0])
                    {
                        _actionObj[0].gameObject.SetActive(isEnable);
                        FileReadAndWriteUtility.UserProfile.scene_view_led = isEnable;
                    }
                    break;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void OnClickParticle(int index)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            string str;
            switch (index)
            {
                case 0:
                    _backGroundCon.SetParticle(-1, out str);
                    _settings.Texts[0].text = "Particle_" + str;
                    break;
                case 1:
                    _backGroundCon.SetParticle(1, out str);
                    _settings.Texts[0].text = "Particle_" + str;
                    break;
            }
        }

        void OnClickWormHolle(int index)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            string str;
            switch (index)
            {
                case 0:
                    _backGroundCon.SetWormHole(-1, out str);
                    _settings.Texts[1].text = "Particle_" + str;
                    break;
                case 1:
                    _backGroundCon.SetWormHole(1, out str);
                    _settings.Texts[1].text = "Particle_" + str;
                    break;
            }
        }

        void OnClickSkyBox(int index)
        {
            if (!_backGroundCon) return;
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            string str;
            switch (index)
            {
                case 0:
                    _backGroundCon.SetCubemap(-1, out str);
                    _settings.Texts[2].text = "SkyBox_" + str;
                    break;
                case 1:
                    _backGroundCon.SetCubemap(1, out str);
                    _settings.Texts[2].text = "SkyBox_" + str;
                    break;
            }
        }

        void IStageMenuService.Dispose()
        {
            _settings.LedButton.onTrigger -= (btn) => OnClick(0, btn.isEnable);

            for (int i = 0; i < _settings.ParticleButtons.Length; i++)
            {
                _settings.ParticleButtons[i].onTrigger -= (btn) => OnClickParticle(i);
            }
            for (int i = 0; i < _settings.WormHolleButtons.Length; i++)
            {
                _settings.WormHolleButtons[i].onTrigger -= (btn) => OnClickWormHolle(i);
            }
            for (int i = 0; i < _settings.SkyBoxButtons.Length; i++)
            {
                _settings.SkyBoxButtons[i].onTrigger -= (btn) => OnClickSkyBox(i);
            }
        }
    }
}