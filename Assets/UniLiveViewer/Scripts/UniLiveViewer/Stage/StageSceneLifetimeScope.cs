using UniLiveViewer.SO;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// シーン遷移とフォルダ内アセット関連
    /// </summary>
    public class StageSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] Light _directionalLight;
        [SerializeField] AudioClipSettings _audioClipSettings;
        [SerializeField] RootAudioSourceService _rootAudioSourceService;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_audioClipSettings);
            builder.RegisterComponent(_rootAudioSourceService);

            builder.Register<AnimationAssetManager>(Lifetime.Singleton);
            builder.Register<TextureAssetManager>(Lifetime.Singleton);
            builder.RegisterComponent(_directionalLight);

            builder.RegisterEntryPoint<StageScenePresenter>();
        }
    }
}
