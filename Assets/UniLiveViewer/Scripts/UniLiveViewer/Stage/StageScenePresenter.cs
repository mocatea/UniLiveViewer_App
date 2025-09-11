using Cysharp.Threading.Tasks;
using NanaCiel;
using System;
using System.Threading;
using UniLiveViewer.SceneLoader;
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
        readonly StageLightingService _stageLightingService;

        [Inject]
        public StageScenePresenter(
            FileAccessManager fileAccessManager,
            AnimationAssetManager animationAssetManager,
            TextureAssetManager textureAssetManager,
            SceneChangeService sceneChangeService,
            StageLightingService stageLightingService)
        {
            _sceneChangeService = sceneChangeService;
            _fileAccessManager = fileAccessManager;
            _animationAssetManager = animationAssetManager;
            _textureAssetManager = textureAssetManager;
            _stageLightingService = stageLightingService;
        }

        void IInitializable.Initialize()
        {
            _stageLightingService.Verify();
            _sceneChangeService.Initialize();
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
