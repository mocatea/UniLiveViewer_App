using System;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class FantasyVillageMenuServie : IStageMenuService
    {
        Transform[] _actionObj = new Transform[1];

        readonly FantasyVillageMenuSettings _settings;
        readonly RootAudioSourceService _audioSourceService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FantasyVillageMenuServie(FantasyVillageMenuSettings settings, RootAudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        void IStageMenuService.Initialize()
        {
            
        }

        void IStageMenuService.OnEnable()
        {
            
        }

        void OnClick(bool isEnable)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            if (_actionObj[0])
            {
                _actionObj[0].gameObject.SetActive(isEnable);
                FileReadAndWriteUtility.UserProfile.scene_fv_light = isEnable;
            }
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void IStageMenuService.Dispose()
        {
            _disposables.Dispose();
        }
    }
}