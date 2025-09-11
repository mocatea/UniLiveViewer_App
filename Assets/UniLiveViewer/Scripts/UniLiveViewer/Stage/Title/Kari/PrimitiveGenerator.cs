using MessagePipe;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Title.Kari
{
    public class PrimitiveGenerator : MonoBehaviour
    {
        [SerializeField] GameObject[] _prefabs;
        [SerializeField] Material _material;
        [SerializeField] float hdrIntensity = 2.0f;
        [SerializeField] Transform[] _parents;

        [SerializeField] float _range_min = 15;
        [SerializeField] float _range_max = 30;
        [SerializeField] float _intervalTime = 1;
        float _timer = 0;

        MaterialPropertyBlock _propBlock;

        void Start()
        {
            _propBlock = new MaterialPropertyBlock(); // フィールド初期化はNG

            var lifetimeScope = LifetimeScope.Find<TitleSceneLifetimeScope>();
            var sceneTransitionSubscriber = lifetimeScope.Container.Resolve<ISubscriber<SceneTransitionMessage>>();
            sceneTransitionSubscriber
                .Subscribe(x =>
                {
                    this.enabled = false;
                }).AddTo(this);
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_intervalTime < _timer)
            {
                Generate();
                _timer = 0;
            }
        }

        void Generate()
        {
            var index = Random.Range(0, _prefabs.Length);
            var go = Instantiate(_prefabs[index]);
            var meshRenderers = go.GetComponentsInChildren<MeshRenderer>();

            var angle = Random.Range(0f, 360f);
            var distance = Random.Range(_range_min, _range_max);
            // XZ座標で配置する位置を計算 (Mathf.SinとMathf.Cosで円周上の座標を決定)
            var spawnPosition = new Vector3(
                transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                transform.position.y,  // 高さはAと同じにする（または別途調整可能）
                transform.position.z + Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            go.transform.parent = _parents[Random.Range(0, 2)];
            go.transform.position = spawnPosition;
            go.transform.localScale = Vector3.one * Random.Range(0.2f, 1.0f);

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var color = new Color(Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f));
                Color.RGBToHSV(color, out float H, out float S, out float V);
                var colorFromHSV = Color.HSVToRGB(H, Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f)) * hdrIntensity;//一定以上の彩度と明度
                _propBlock.SetColor("_BaseColor", colorFromHSV);
                _propBlock.SetColor("_EmissionColor", colorFromHSV);
                meshRenderers[i].sharedMaterial = _material;
                meshRenderers[i].SetPropertyBlock(_propBlock);
            }
        }
    }
}