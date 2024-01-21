﻿using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace UniLiveViewer.SceneLoader
{
    public class ViewerScene : IScene
    {
        const int BufferTime = 5000;
        const string SceneName = "ViewerScene";

        public ViewerScene()
        {

        }

        async UniTask IScene.BeginAsync(CancellationToken token)
        {
            FileReadAndWriteUtility.UserProfile.LastSceneName = SceneName;

            //完全非同期は無理
            var async = SceneManager.LoadSceneAsync(SceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(BufferTime, cancellationToken: token);
            async.allowSceneActivation = true;

            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);//完了したら更新
        }


        SceneType IScene.GetSceneType() => SceneType.VIEWER;

        string IScene.GetVisualName() => "★ViewerScene★";
    }
}