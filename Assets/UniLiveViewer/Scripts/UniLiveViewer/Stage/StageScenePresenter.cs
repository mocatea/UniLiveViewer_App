using Cysharp.Threading.Tasks;
using NanaCiel;
using System;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.SO;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// シーン遷移・ファイル準備のみ
    /// </summary>
    public class StageScenePresenter : IInitializable ,IAsyncStartable
    {
        readonly SceneChangeService _sceneChangeService;
        readonly FileAccessManager _fileAccessManager;
        readonly AnimationAssetManager _animationAssetManager;
        readonly TextureAssetManager _textureAssetManager;
        readonly SceneInitialSettings _sceneInitialSettings;
        readonly StageLightingService _stageLightingService;

        [Inject]
        public StageScenePresenter(
            SceneChangeService sceneChangeService,
            FileAccessManager fileAccessManager,
            AnimationAssetManager animationAssetManager,
            TextureAssetManager textureAssetManager,
            SceneInitialSettings sceneInitialSettings,
            StageLightingService stageLightingService)
        {
            _sceneChangeService = sceneChangeService;
            _fileAccessManager = fileAccessManager;
            _animationAssetManager = animationAssetManager;
            _textureAssetManager = textureAssetManager;
            _sceneInitialSettings = sceneInitialSettings;
            _stageLightingService = stageLightingService;
        }

        void IInitializable.Initialize()
        {
            _stageLightingService.Verify();
            _sceneChangeService.Initialize();

            var sceneData = _sceneInitialSettings.GetSettingData(SceneChangeService.GetSceneType);
            _stageLightingService.ChangeLightColor(sceneData.Light.Color);
            _stageLightingService.ChangeLightIntensity(sceneData.Light.Intensity);
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            await _fileAccessManager.PreparationStartAsync(cancellation).OnError(OnFolderError);
            _animationAssetManager.Setup();
            _textureAssetManager.Start();
            await _textureAssetManager.CacheThumbnailsAsync(cancellation).OnError(OnThumbnailsError);
            _fileAccessManager.PreparationEnd();
        }

        void OnFolderError(Exception e)
        {
            Debug.Log($"フォルダ準備エラー:{e}");
        }

        void OnThumbnailsError(Exception e)
        {
            Debug.Log($"サムネイルチェックエラー:{e}");
        }
    }
}
