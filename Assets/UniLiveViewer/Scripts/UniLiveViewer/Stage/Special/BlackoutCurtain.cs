using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// シーン遷移時のPlayerの視界を遮る
    /// TODO: まだ仮
    /// </summary>
    public class BlackoutCurtain : MonoBehaviour
    {
        readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        readonly int ScalaId = Shader.PropertyToID("_Scala");

        [SerializeField] LoadAnimation _loadAnimation;
        [SerializeField] Renderer _cutoffRenderer;
        [SerializeField] Renderer _brackRenderer;

        [SerializeField] TextMesh[] _vmdErrorText = new TextMesh[2];

        [SerializeField] AnimationCurve _animationCurve;

        MaterialPropertyBlock _materialPropertyBlock;
        Color _color;
        CancellationToken _cancellation;

        void Start()
        {
            _cancellation = this.GetCancellationTokenOnDestroy();

            //不透明黒
            _color = new Color(0, 0, 0, 1);
            _materialPropertyBlock = new MaterialPropertyBlock();
            _brackRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor(BaseColorId, _color);
            _brackRenderer.SetPropertyBlock(_materialPropertyBlock);

            _brackRenderer.enabled = true;
            _cutoffRenderer.enabled = false;

            foreach (var textMesh in _vmdErrorText)
            {
                if (textMesh.gameObject.activeSelf) textMesh.gameObject.SetActive(false);
            }

            // 演出開始
            _loadAnimation.gameObject.SetActive(true);
        }

        /// <summary>
        /// エラーメッセージを表示　←警告やめるので削除予定
        /// </summary>
        public void ShowErrorMessage()
        {
            //Debug.Log("読み込み失敗発生:" + FileReadAndWriteUtility.UserProfile.LanguageCode);

            //if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);

            //int index = FileReadAndWriteUtility.UserProfile.LanguageCode - 1;
            //vmdError[index].gameObject.SetActive(true);
        }

        /// <summary>
        /// 演出終了
        /// </summary>
        public async UniTaskVoid Ending()
        {
            await UniTask.Delay(300, cancellationToken: _cancellation);

            //まずloadingアニメーションを消す
            _loadAnimation.gameObject.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);

            //暗転から徐々に再開
            _color = _materialPropertyBlock.GetColor(BaseColorId);
            _color.a = 1;//不透明

            while (_color.a >= 0.0f)
            {
                _color.a -= Time.deltaTime;

                _materialPropertyBlock.SetColor(BaseColorId, _color);
                _brackRenderer.SetPropertyBlock(_materialPropertyBlock);
                await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);
            }
        }

        /// <summary>
        /// 暗転させる
        /// </summary>
        public async UniTask FadeoutAsync(CancellationToken cancellation)
        {
            _brackRenderer.enabled = false;
            _cutoffRenderer.enabled = true;
            if (_loadAnimation.gameObject.activeSelf) _loadAnimation.gameObject.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);

            //閉幕演出
            float t = 0;
            while (t < 2.5f)
            {
                _cutoffRenderer.sharedMaterial.SetFloat(ScalaId, _animationCurve.Evaluate(t));
                t += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);
            }
            _brackRenderer.enabled = true;
            _color.a = 1;//不透明
            _materialPropertyBlock.SetColor(BaseColorId, _color);
            _brackRenderer.SetPropertyBlock(_materialPropertyBlock);

            _cutoffRenderer.sharedMaterial.SetFloat(ScalaId, 0);
            _cutoffRenderer.enabled = false;

            //ローディングアニメーション
            _loadAnimation.gameObject.SetActive(true);
            await UniTask.Delay(200, cancellationToken: cancellation);
        }
    }
}