using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace UniLiveViewer.SceneLoader
{
    public class BeyondTheBlueScene : IScene
    {
        const int BufferTime = 5000;
        const string SceneName = "HorizonLine_Test";
        public BeyondTheBlueScene()
        {
        }

        async UniTask IScene.BeginAsync(CancellationToken token)
        {
            //完全非同期は無理
            var async = SceneManager.LoadSceneAsync(SceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(BufferTime, cancellationToken: token);
            async.allowSceneActivation = true;
        }

        string IScene.GetVisualName() => "★Beyond The Blue★";
    }
}
