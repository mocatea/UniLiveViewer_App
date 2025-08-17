using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// VRMサムネメニュー画面
    /// 
    /// TODO: ほぼ機能ないが今度作り直す
    /// </summary>
    public class VRMSwitchController : MonoBehaviour
    {
        /// <summary>
        /// 各ページ
        /// </summary>
        [SerializeField] Transform[] _pageTransform;

        [Space(1), Header("＜1ページ＞")]
        [SerializeField] TextMesh[] _textDirectory;
        [Space(1), Header("＜2ページ＞")]
        [SerializeField] Button_Base _btnApply;
        [SerializeField] PrefabEditor _prefabEditor;
        [Space(1), Header("＜3ページ＞")]
        [SerializeField] TextMesh _textErrorResult;

        [Header("＜その他＞")]
        //特殊表情用サウンド
        [SerializeField] AudioClip[] _specialFaceAudioClip;

        //ファイルアクセスとサムネの管理
        FileAccessManager _fileManager;
        RootAudioSourceService _audioSourceService;
        TextureAssetManager _textureAssetManager;
        IPublisher<VRMMenuShowMessage> _publisher;

        public async UniTask InitializeAsync(
            FileAccessManager fileAccessManager,
            RootAudioSourceService audioSourceService,
            TextureAssetManager textureAssetManager,
            IPublisher<VRMMenuShowMessage> publisher,
            CancellationToken cancellation)
        {
            _fileManager = fileAccessManager;
            _audioSourceService = audioSourceService;
            _textureAssetManager = textureAssetManager;
            _publisher = publisher;
            //コールバック登録・・・2ページ目
            //_btnApply.onTrigger += (btn) => PrefabApply(btn, cancellation).Forget();
            //_prefabEditor.onCurrentUpdate += () => { _audioSourceService.PlayOneShot(AudioSE.ButtonClick); };

            await UniTask.CompletedTask;
        }

        /// <summary>
        /// カレントページで開き直す（初期化）
        /// </summary>
        public void InitPage(int currentPage)
        {
            //SetEnableRoot(true);

            //ページスイッチ
            for (int i = 0; i < _pageTransform.Length; i++)
            {
                if (currentPage == i)
                {
                    if (!_pageTransform[i].gameObject.activeSelf) _pageTransform[i].gameObject.SetActive(true);
                }
                else
                {
                    if (_pageTransform[i].gameObject.activeSelf) _pageTransform[i].gameObject.SetActive(false);
                }
            }

            //各初期化処理
            switch (currentPage)
            {
                case 0:
                    //フォルダパスの表示を更新
                    //_textDirectory[0].text = $"({PathsInfo.GetFullPath(FolderType.Actor)}/)"; //不要
                    _textDirectory[1].text = $"/Download...[{_fileManager.CountVRM(PathsInfo.GetDownloadFolderPath() + "/")} VRMs]";
                    break;
                case 1:
                    //prefabEditor.Init();

                    //表示を更新
                    //MaterialInfoUpdate();
                    break;
                case 2:
                    break;
            }
        }

        async UniTask OnError(CancellationToken cancellation)
        {
            //if (_currentVrmInstance) Destroy(_currentVrmInstance);

            //_vrmLoaderUI.SetUIActive(false);

            //errorページ
            //InitPage(2);

            //Debug.LogError(exception);
            //System.IO.StringReader rs = new System.IO.StringReader(exception.ToString());
            //_textErrorResult.text = $"{rs.ReadLine()}";//1行まで
            _textErrorResult.text = "何らかの理由で読み込み失敗";
            await UniTask.Delay(5000, cancellationToken: cancellation);
        }

        //public void VRMEditing(CharaController _vrmModel)
        //{
        //    ////編集対象へ
        //    //_prefabEditor.SetEditingTarget(_vrmModel);
        //    ////マテリアル設定ページへ
        //    ////InitPage(1);
        //}

        /// <summary>
        /// 設定の確定
        /// </summary>
        /// <param name="btn"></param>
        async UniTask PrefabApply(Button_Base btn, CancellationToken cancellation)
        {
            //cancellation.ThrowIfCancellationRequested();

            ////クリック音
            //_audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            //var vrm = _prefabEditor.EditTarget;

            //if (vrm.AnimationMode == CharaEnums.ANIMATION_MODE.VMD)
            //{
            //    var vmdPlayer = vrm.GetComponent<VMDPlayer_Custom>();
            //    vmdPlayer.ClearBaseAndSyncData();
            //}
            //else if (vrm.AnimationMode == CharaEnums.ANIMATION_MODE.CLIP)
            //{
            //    vrm.GetComponent<Animator>().enabled = false;
            //}
            //vrm.SetState(CharaEnums.STATE.NULL, null);

            ////_vrmLoaderUI.SetVRMToPrefab(vrm);

            //await UniTask.Delay(1000, cancellationToken: cancellation);
            //cancellation.ThrowIfCancellationRequested();

            //vrm.SetEnabelSpringBones(false);//Prefab化で値が残ってしまうので無効化
            //vrm.GetComponent<Animator>().enabled = true;
            //vrm.AnimationMode = CharaEnums.ANIMATION_MODE.CLIP;
            //vrm.gameObject.SetActive(false);

            //await UniTask.Yield(cancellation);
            //cancellation.ThrowIfCancellationRequested();

            //_addPrefabStream.OnNext(vrm);

            //UIを非表示にする
            //SetEnableRoot(false);
        }

        /// <summary>
        /// VRMプレハブを削除する
        /// </summary>
        /// <param name="id"></param>
        public void ClearVRMPrefab(int id)
        {
            //_vrmLoaderUI.DeleteVRMPrefab(id);
        }

        /// <summary>
        /// ダウンロードフォルダからVRMをコピーしてくる
        /// </summary>
        public async void OnClick_VRMCopy()
        {
            var cancel = this.GetCancellationTokenOnDestroy();
            try
            {
                await _textureAssetManager.CopyVRMtoActorFolderAsync(PathsInfo.GetDownloadFolderPath() + "/", cancel);
                _publisher.Publish(new VRMMenuShowMessage(0));
            }
            catch
            {
                _textDirectory[1].text = "VRM Copy Error...";
            }
        }
    }
}