using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace UniLiveViewer.SceneLoader
{
    public class ViewerScene : IScene
    {
        const string SceneName = "ViewerScene";

        public ViewerScene()
        {
        }

        async UniTask IScene.BeginAsync(CancellationToken token)
        {
            //完全非同期は無理
            var async = SceneManager.LoadSceneAsync(SceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(SceneConstant.TransitionBufferTime, cancellationToken: token);
            async.allowSceneActivation = true;
        }

        string IScene.GetVisualName() => "ViewerScene";
    }
}
