using System;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Dance
{
    public class DanceMenuService
    {
        readonly DanceMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public DanceMenuService(
            DanceMenuSettings settings,
            RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        public void Initialize()
        {
            _settings.VMDSmoothButton.isEnable = FileReadAndWriteUtility.UserProfile.IsSmoothVMD;
            _settings.VMDSmoothButton.OnTriggerAsObservable()
                .Subscribe(OnChangeVMDSmooth).AddTo(_disposables);

            _settings.VMDScaleSlider.Value = FileReadAndWriteUtility.UserProfile.VMDScale;
        }

        void OnChangeVMDSmooth(Button_Base button_Base)
        {
            FileReadAndWriteUtility.UserProfile.IsSmoothVMD = _settings.VMDSmoothButton.isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        public void OnUpdateVMDScale(float value)
        {
            _settings.VMDScaleText.text = $"{value:0.000}";
        }

        public void OnUnControledVMDScale()
        {
            FileReadAndWriteUtility.UserProfile.VMDScale = float.Parse(_settings.VMDScaleSlider.Value.ToString("f3"));
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}