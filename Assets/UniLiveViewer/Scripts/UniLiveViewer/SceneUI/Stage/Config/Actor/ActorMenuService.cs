using UniLiveViewer.Timeline;
using VContainer;

namespace UniLiveViewer.Menu.Config.Actor
{
    public class ActorMenuService
    {
        readonly RootAudioSourceService _audioSourceService;

        [Inject]
        public ActorMenuService(
            RootAudioSourceService audioSourceService)
        {
            _audioSourceService = audioSourceService;
        }

        public void ApplyActorSizeValue(float value)
        {
            FileReadAndWriteUtility.UserProfile.InitCharaSize = float.Parse(value.ToString("f2"));
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void ApplyFallingShadowSizeValue(float value)
        {
            FileReadAndWriteUtility.UserProfile.CharaShadowSize = float.Parse(value.ToString("f2"));
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void ApplyFallingShadowTypeValue(SHADOWTYPE type)
        {
            FileReadAndWriteUtility.UserProfile.CharaShadowType = (int)type;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }
    }
}