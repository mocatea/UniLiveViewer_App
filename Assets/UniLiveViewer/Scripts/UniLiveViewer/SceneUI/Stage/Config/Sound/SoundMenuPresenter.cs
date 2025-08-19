using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Timeline;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Sound
{
    public class SoundMenuPresenter : IStartable, IDisposable
    {
        readonly SoundMenuService _soundMenuService;
        readonly RootAudioSourceService _audioSourceService;
        readonly SpectrumConverter _spectrumConverter;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SoundMenuPresenter(
            SoundMenuService soundMenuService,
            RootAudioSourceService audioSourceService,
            SpectrumConverter spectrumConverter)
        {
            _soundMenuService = soundMenuService;
            _audioSourceService = audioSourceService;
            _spectrumConverter = spectrumConverter;
        }

        void IStartable.Start()
        {
            _soundMenuService.Initialize
                (_audioSourceService.MasterVolumeRate * 100,
                _audioSourceService.BGMVolumeRate.Value * 100,
                _audioSourceService.SEVolumeRate.Value * 100,
                _audioSourceService.AmbientVolumeRate.Value * 100,
                _spectrumConverter.Gain,
                _spectrumConverter.Smoothness,
                _audioSourceService.FootStepsVolumeRate.Value * 100);

            _soundMenuService.Master
                .SkipLatestValueOnSubscribe()
                .Subscribe(_audioSourceService.SetMasterVolume)
                .AddTo(_disposables);
            _soundMenuService.BGM
                .SkipLatestValueOnSubscribe()
                .Subscribe(_audioSourceService.SetBGMVolume)
                .AddTo(_disposables);
            _soundMenuService.SE
                .SkipLatestValueOnSubscribe()
                .Subscribe(_audioSourceService.SetSEVolume)
                .AddTo(_disposables);
            _soundMenuService.Ambient
                .SkipLatestValueOnSubscribe()
                .Subscribe(_audioSourceService.SetAmbientVolume)
                .AddTo(_disposables);
            _soundMenuService.FootSteps
                .SkipLatestValueOnSubscribe()
                .Subscribe(_audioSourceService.SetFootStepsVolume)
                .AddTo(_disposables);
            _soundMenuService.SpectrumGain
               .SkipLatestValueOnSubscribe()
               .Subscribe(_spectrumConverter.SetGain)
               .AddTo(_disposables);
            _soundMenuService.SpectrumSmoothness
               .SkipLatestValueOnSubscribe()
               .Subscribe(_spectrumConverter.SetSmoothness)
               .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
