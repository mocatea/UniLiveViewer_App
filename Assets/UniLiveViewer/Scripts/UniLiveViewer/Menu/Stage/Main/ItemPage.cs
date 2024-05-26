﻿using Cysharp.Threading.Tasks;
using UniLiveViewer.Player;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class ItemPage : MonoBehaviour
    {
        const int SUBPAGE_ITEMS_ROW = 2;
        const int SUBPAGE_ITEMS_COL = 3;
        int SUBPAGE_ITEMS_MAX = SUBPAGE_ITEMS_ROW * SUBPAGE_ITEMS_COL;
        static readonly Vector2[] itemOffsetPos = {
            new Vector2(-0.11f, 0.08f), new Vector2(0.09f, 0.08f), new Vector2(0.3f, 0.08f),
            new Vector2(-0.11f,-0.10f), new Vector2(0.09f,-0.10f),new Vector2(0.3f,-0.10f),
        };
        static readonly Quaternion reverseQuaternion = Quaternion.Euler(new Vector3(0, 180, 0));

        [System.Serializable]
        public class DecorationItems
        {
            public DecorationItemInfo[] ItemPrefab;
        }

        [SerializeField] MenuManager _menuManager;

        [SerializeField] Button_Base[] _itemButton = new Button_Base[2];
        [SerializeField] TextMesh _textMesh;

        [SerializeField] PageController _pageController;
        [SerializeField] Transform _itemMaterialAnchor;
        [SerializeField] GameObject _itemMaterialPrefab;
        [Header("確認用")]
        [SerializeField] int[] _currentSubPage;

        [SerializeField] DecorationItemSettings _decorationItemSettings;

        AudioSourceService _audioSourceService;
        PassthroughService _passthroughService;

        [Inject]
        public void Construct(
            AudioSourceService audioSourceService,
            PassthroughService passthroughService)
        {
            _audioSourceService = audioSourceService;
            _passthroughService = passthroughService;
        }

        public void OnStart()
        {
            _currentSubPage = new int[_decorationItemSettings.ItemPrefab.Length];

            for (int i = 0; i < _itemButton.Length; i++)
            {
                _itemButton[i].onTrigger += OnClickItemIndex;
            }
            _pageController.ChangePageAsObservable
                .Subscribe(_ => Initialize()).AddTo(this);
        }

        void Initialize()
        {
            EnablePassthrough(_passthroughService.IsInsightPassthroughEnabled());

            //アイテム数に応じてページ送りボタンの表示切替
            var itemLength = _decorationItemSettings.ItemPrefab[_pageController.Current].ItemPrefab.Length;
            var isOver = itemLength > SUBPAGE_ITEMS_MAX;
            for (int i = 0; i < _itemButton.Length; i++)
            {
                if (_itemButton[i].gameObject.activeSelf != isOver) _itemButton[i].gameObject.SetActive(isOver);
            }

            //アクティブページのアイテムを生成
            IfNeededGenerateItems();
        }

        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        void EnablePassthrough(bool isEnable)
        {
            if (_pageController.BtnTab[5].gameObject.activeSelf == isEnable) return;

            _pageController.BtnTab[5].gameObject.SetActive(isEnable);
            _pageController.BtnTab[5].isEnable = false;
        }

        /// <summary>
        /// アイテム変更ボタン
        /// </summary>
        void OnClickItemIndex(Button_Base btn)
        {
            for (int i = 0; i < 2; i++)
            {
                if (_itemButton[i] != btn) continue;

                if (i == 0)
                {
                    EvaluateItemIndex(-1);
                    return;
                }
                else if (i == 1)
                {
                    EvaluateItemIndex(1);
                    return;
                }
            }
        }

        void EvaluateItemIndex(int moveIndex)
        {
            var nowSubPage = _currentSubPage[_pageController.Current];
            var itemLength = _decorationItemSettings.ItemPrefab[_pageController.Current].ItemPrefab.Length;
            var maxSubPage = itemLength / SUBPAGE_ITEMS_MAX + (itemLength % SUBPAGE_ITEMS_MAX == 0 ? 0 : 1) - 1;

            //元ページのアイテムを削除
            DeleteAllItemsCurrentPage();

            //サブページ移動
            nowSubPage += moveIndex;
            if (nowSubPage > maxSubPage) nowSubPage = 0;
            else if (nowSubPage < 0) nowSubPage = maxSubPage;
            _currentSubPage[_pageController.Current] = nowSubPage;

            //アクティブページのアイテムを生成
            IfNeededGenerateItems();

            //クリック音
            _audioSourceService.PlayOneShot(0);
        }

        /// <summary>
        /// 現在ページのアイテムを全削除
        /// </summary>
        void DeleteAllItemsCurrentPage()
        {
            var targetPageAnchor = _pageController.GetCurrentPageAnchor();
            var maxCount = targetPageAnchor.childCount;

            for (int i = 0; i < maxCount; i++)
            {
                Destroy(targetPageAnchor.GetChild(maxCount - i - 1).gameObject);//常に末尾を削除
            }
        }

        /// <summary>
        /// 現在の選択カテゴリとサブページを基準にアイテムが無ければ生成
        /// </summary>
        void IfNeededGenerateItems()
        {
            var nowPageIndex = _currentSubPage[_pageController.Current];
            var itemPrefabLength = _decorationItemSettings.ItemPrefab[_pageController.Current].ItemPrefab.Length;
            var maxPageLength = itemPrefabLength / SUBPAGE_ITEMS_MAX + (itemPrefabLength % SUBPAGE_ITEMS_MAX == 0 ? 0 : 1) - 1;
            var nowPageMinItemIndex = nowPageIndex * SUBPAGE_ITEMS_MAX;

            _textMesh.text = $"{nowPageIndex + 1} / {maxPageLength + 1}";

            var currentItemPrefabs = _decorationItemSettings.ItemPrefab[_pageController.Current];

            for (int i = 0; i < SUBPAGE_ITEMS_MAX; i++)
            {
                var itemIndex = nowPageMinItemIndex + i;
                if (itemIndex >= currentItemPrefabs.ItemPrefab.Length) return;

                //重複生成させないために固有名をつける
                var itemName = $"{nowPageIndex}-{itemIndex}";
                if (CheckGenerated(itemName)) continue;

                var instance = Instantiate(currentItemPrefabs.ItemPrefab[itemIndex]).transform;
                instance.name = itemName;
                instance.parent = _pageController.GetCurrentPageAnchor();
                instance.localPosition = new Vector3(itemOffsetPos[i].x, itemOffsetPos[i].y, 0);
                instance.localRotation = Quaternion.identity * reverseQuaternion;
                instance.localScale *= 0.5f;
            }
        }

        /// <summary>
        /// アイテムが既出か確認
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        bool CheckGenerated(string targetName)
        {
            foreach (Transform instance in _pageController.GetCurrentPageAnchor())
            {
                if (instance.name == targetName) return true;
            }
            return false;
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) EvaluateItemIndex(1);
            if (Input.GetKeyDown(KeyCode.K)) EvaluateItemIndex(-1);
        }
    }
}