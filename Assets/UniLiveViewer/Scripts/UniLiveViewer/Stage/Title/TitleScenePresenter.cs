using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Title
{
    public class TitleScenePresenter : IAsyncStartable
    {
        readonly AudioSource _mainAudioSource;
        readonly TitleSceneSettings _titleSceneSettings;
        readonly ISubscriber<SceneTransitionMessage> _sceneTransitionSubscriber;
        readonly CompositeDisposable _disposable = new();

        [Inject]
        public TitleScenePresenter(
            AudioSource mainAudioSource,
            TitleSceneSettings titleSceneSettings,
            ISubscriber<SceneTransitionMessage> sceneTransitionSubscriber)
        {
            _mainAudioSource = mainAudioSource;
            _titleSceneSettings = titleSceneSettings;
            _sceneTransitionSubscriber = sceneTransitionSubscriber;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _titleSceneSettings.AppVersionText.text = "ver " + Application.version;

            _sceneTransitionSubscriber
                .Subscribe(x =>
                {
                    EndFadeAsync(cancellation).Forget();
                }).AddTo(_disposable);

            StartFadeAsync(cancellation).Forget();
            StartScalingEffectAsync(cancellation).Forget();

            // 保留
            //_mainAudioSource.playOnAwake = false;
            //_mainAudioSource.loop = true;
            //await UniTask.Delay(1000);
            //_mainAudioSource.time = 3.5f;
            //_mainAudioSource.Play();

            await UniTask.CompletedTask;
        }

        async UniTask StartFadeAsync(CancellationToken cancellation)
        {

            _titleSceneSettings.SpriteRenderer.color = new Color(1, 1, 1, 0);
            _titleSceneSettings.AppVersionText.color = new Color(1, 1, 1, 0);
            await UniTask.Delay(12000, cancellationToken: cancellation);//実機ベースだとこの調整
            var timer = 0.0f;
            while (timer < 1.0)
            {
                timer += Time.deltaTime;
                _titleSceneSettings.SpriteRenderer.color = new Color(1, 1, 1, timer);
                _titleSceneSettings.AppVersionText.color = new Color(1, 1, 1, timer);
                await UniTask.Yield(cancellation);
            }
        }

        async UniTask StartScalingEffectAsync(CancellationToken cancellation)
        {
            var scalingEffectmesh = _titleSceneSettings.ScalingEffect.GetComponent<MeshRenderer>();
            scalingEffectmesh.enabled = false;
            await UniTask.Delay(11500, cancellationToken: cancellation);//実機ベースだとこの調整
            scalingEffectmesh.enabled = true;
            scalingEffectmesh.material.SetFloat("_Scale", 0);
            var timer = 0.0f;
            var startSize = 200;
            var scaling = startSize * 2;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                scalingEffectmesh.material.SetFloat("_Scale", startSize - (timer * scaling));
                await UniTask.Yield(cancellation);
            }
            scalingEffectmesh.enabled = false;
        }


        async UniTask EndFadeAsync(CancellationToken cancellation)
        {
            var timer = _mainAudioSource.volume;
            await UniTask.Delay(2000, cancellationToken: cancellation);
            while (0.0f < timer)
            {
                timer -= Time.deltaTime * 0.12f;
                _mainAudioSource.volume = timer;
                await UniTask.Yield(cancellation);
            }
        }
    }
}