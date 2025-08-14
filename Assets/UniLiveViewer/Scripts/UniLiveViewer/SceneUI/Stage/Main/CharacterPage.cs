using Cysharp.Threading.Tasks;
using MessagePipe;
using NanaCiel;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// TODO: いつか整理する
    /// </summary>
    public class CharacterPage : MonoBehaviour
    {
        [SerializeField] Stage.LoadAnimation _loadingAnimation;
        [SerializeField] TextMesh _pleasePushText;
        [SerializeField] TextMesh _vrmLoadFailureText;
        [SerializeField] TextMesh _actorMaxText;
        [SerializeField] TextMesh _actorHeigthText;
        [SerializeField] TextMesh _actorBoneCountText;
        [SerializeField] TextMesh _actorMaterialsCountText;
        [SerializeField] TextMesh _actorPolygonsText;

        MenuManager _menuManager;
        [Header("--- Preset or Custom ---")]
        [SerializeField] Button_Switch[] _switchChara = new Button_Switch[2];
        [SerializeField] Button_Switch[] _switchAnime = new Button_Switch[2];

        [Header("--- JumpList ---")]
        /// <summary>
        /// キャラ、モーション、リップ
        /// </summary>
        [SerializeField] Button_Base[] _btnJumpList;

        [Header("--- MoveIndex ---")]
        [SerializeField] Transform _vrmOptionAnchor;
        [SerializeField] Transform _vmdAnchor;
        [SerializeField] Button_Base[] _btnChara = new Button_Base[2];
        [SerializeField] Button_Base[] _btnAnime = new Button_Base[2];

        [Header("--- ---")]
        [SerializeField] Button_Base[] _btnOffset = new Button_Base[2];
        [SerializeField] Button_Switch _switchReverse;
        [SerializeField] Button_Base _btnDeleteAll;
        [SerializeField] TextMesh[] _textMeshs;

        [Header("--- Slider ---")]
        [SerializeField] SliderGrabController _sliderOffset;
        [SerializeField] SliderGrabController _sliderHeadLook;
        [SerializeField] SliderGrabController _sliderEyeLook;
        [SerializeField] TextMesh[] _lookAtText;

        [Header("--- VRM用 ---")]
        [SerializeField] Button_Base _btnVRMSetting;
        [SerializeField] Button_Base _btnVRMDelete;
        [SerializeField] Button_Base _btnVRM10Mode;//0.xと1.0切り替え
        [SerializeField] Button_Base _btnFacialActive;
        [SerializeField] Button_Base _btnLipSyncActive;

        public IReadOnlyReactiveProperty<int> FBXIndex => _fbxIndex;
        readonly ReactiveProperty<int> _fbxIndex = new();
        public IReadOnlyReactiveProperty<int> VRMIndex => _vrmIndex;
        readonly ReactiveProperty<int> _vrmIndex = new();
        CurrentMode _currentActorMode = CurrentMode.PRESET;

        public IReadOnlyReactiveProperty<bool> IsReverse => _isReverse;
        readonly ReactiveProperty<bool> _isReverse = new();

        public IReadOnlyReactiveProperty<int> ClipIndex => _clipIndex;
        readonly ReactiveProperty<int> _clipIndex = new();
        public IReadOnlyReactiveProperty<int> VMDIndex => _vmdIndex;
        readonly ReactiveProperty<int> _vmdIndex = new();
        CurrentMode _animationMode = CurrentMode.PRESET;

        /// <summary> 操作受付可能か </summary>
        bool _interactable;

        PlayableBinderService _playableBinderService;
        PresetResourceData _presetResourceData;
        ActorEntityManagerService _actorEntityManagerService;
        AnimationAssetManager _animationAssetManager;
        IPublisher<AllActorOperationMessage> _allPublisher;
        IPublisher<ActorOperationMessage> _publisher;
        RootAudioSourceService _audioSourceService;
        BookService _bookService;

        [Inject]
        public void Construct(
            PlayableBinderService playableBinderService,
            MenuManager menuManager,
            PresetResourceData presetResourceData,
            ActorEntityManagerService actorEntityManagerService,
            AnimationAssetManager animationAssetManager,
            IPublisher<AllActorOperationMessage> actorOperationPublisher,
            IPublisher<ActorOperationMessage> publisher,
            RootAudioSourceService audioSourceService,
            BookService bookService)
        {
            _playableBinderService = playableBinderService;
            _menuManager = menuManager;
            _presetResourceData = presetResourceData;
            _actorEntityManagerService = actorEntityManagerService;
            _animationAssetManager = animationAssetManager;
            _allPublisher = actorOperationPublisher;
            _publisher = publisher;
            _audioSourceService = audioSourceService;
            _bookService = bookService;
        }

        async void OnEnable()
        {
            var data = _playableBinderService.BindingData[TimelineConstants.PortalIndex];
            if (data != null) return;

            //初期召喚と未生成を想定した自動生成
            await UniTask.Delay(250);
            EvaluateActorIndex(0);
        }

        public void OnStart()
        {
            //ジャンプリスト
            foreach (var e in _btnJumpList)
            {
                e.onTrigger += OpenJumplist;
            }

            //その他
            for (int i = 0; i < _btnChara.Length; i++) _btnChara[i].onTrigger += OnMoveIndexActor;
            for (int i = 0; i < _btnAnime.Length; i++) _btnAnime[i].onTrigger += OnMoveIndexAnimation;

            for (int i = 0; i < _btnOffset.Length; i++)
            {
                _btnOffset[i].onTrigger += OnClickVMDOffset;
            }

            for (int i = 0; i < _switchChara.Length; i++)
            {
                _switchChara[i].isEnable = (i == 0);
                _switchChara[i].onTrigger += OnClickSwitchChara;
            }
            for (int i = 0; i < _switchAnime.Length; i++)
            {
                _switchAnime[i].isEnable = (i == 0);
                _switchAnime[i].onTrigger += OnClickSwitchAnime;
            }

            _switchReverse.onTrigger += (b) => OnChangeReverseAnimation(b);
            _switchReverse.isEnable = false;

            _sliderOffset.ValueAsObservable
                .Subscribe(value =>
                {
                    var baseMotion = _textMeshs[1].text;
                    FileReadAndWriteUtility.SetMotionOffset(baseMotion, (int)value);
                    _textMeshs[3].text = $"{value:0000}";
                }).AddTo(this);

            _sliderOffset.EndDriveAsObservable
                .Subscribe(_ => FileReadAndWriteUtility.SaveMotionOffset()).AddTo(this);

            _sliderHeadLook.ValueAsObservable
                .Subscribe(value =>
                {
                    _lookAtText[0].text = $"{value:0.00}";

                    var data = _playableBinderService.BindingData[TimelineConstants.PortalIndex];
                    if (data == null) return;
                    //顔の向く量をセット
                    var lookAtAllocator = data.ActorEntity.ActorEntity().Value.LookAtService;
                    lookAtAllocator.SetHeadWeight(value);
                }).AddTo(this);
            _sliderEyeLook.ValueAsObservable
                .Subscribe(value =>
                {
                    _lookAtText[1].text = $"{value:0.00}";

                    var data = _playableBinderService.BindingData[TimelineConstants.PortalIndex];
                    if (data == null) return;
                    //目の向く量をセット
                    var lookAtAllocator = data.ActorEntity.ActorEntity().Value.LookAtService;
                    lookAtAllocator.SetEyeWeight(value);
                }).AddTo(this);
            //_btnVRMSetting.onTrigger += VRMSetting;
            _btnVRMDelete.onTrigger += OnClickVRMDelete;
            _btnVRM10Mode.onTrigger += OnClickVRMMode;
            _btnDeleteAll.onTrigger += OnClickDeleteAllActors;
            _btnFacialActive.onTrigger += OnClickFacialExpression;
            _btnLipSyncActive.onTrigger += OnClickFacialExpression;

            _btnVRM10Mode.isEnable = FileReadAndWriteUtility.UserProfile.IsVRM10;

            //_vrmSelectUI.AddPrefabAsObservable
            //    .Subscribe(x =>
            //    {
            //        _timeline.ClearCaracter();
            //        //VRMのPrefabを差し替える
            //        _generatorPortal.ChangeCurrentVRM(x);

            //        if (_currentActorMode == CurrentMode.CUSTOM)
            //        {
            //            _vrmIndex.SetValueAndForceNotify(_vrmIndex.Value);
            //        }
            //        //var instance = Instantiate(vrm).GetComponent<CharaController>();
            //        //instance.SetState(CharaController.CHARASTATE.MINIATURE, generatorPortal.transform);
            //    }).AddTo(_disposable);

            if (_vmdAnchor.gameObject.activeSelf) _vmdAnchor.gameObject.SetActive(false);
            if (_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(false);

            _interactable = true;
        }

        // 待ち中にタブ押下はカバーできていないが許容
        public async void OnLoadedVRM(VRMLoadResultData vrm)
        {
            if (vrm.Value != null) return;

            // ロード失敗時は削除(ex:0.xモデルを1.0モードでロード)
            _loadingAnimation.gameObject.SetActive(false);
            _vrmLoadFailureText.gameObject.SetActive(true);

            _interactable = false;
            await UniTask.Delay(3000);
            _interactable = true;
            OnClickVRMDelete(null);
        }

        public void OnJumpSelect((JumpList.TARGET, int) select)
        {
            var target = select.Item1;
            var index = select.Item2;

            switch (target)
            {
                case JumpList.TARGET.CHARA:
                    if (_currentActorMode == CurrentMode.PRESET)
                    {
                        var moveIndex = index - _fbxIndex.Value;
                        EvaluateActorIndex(moveIndex);
                    }
                    else if (_currentActorMode == CurrentMode.CUSTOM)
                    {
                        var moveIndex = index - _vrmIndex.Value;
                        EvaluateActorIndex(moveIndex);
                    }
                    break;
                case JumpList.TARGET.ANIME:
                    if (_animationMode == CurrentMode.PRESET)
                    {
                        var moveIndex = index - _clipIndex.Value;
                        EvaluateAnimationIndex(moveIndex);
                    }
                    else if (_animationMode == CurrentMode.CUSTOM)
                    {
                        var moveIndex = index - _vmdIndex.Value;
                        EvaluateAnimationIndex(moveIndex);
                    }
                    break;
                case JumpList.TARGET.VMD_LIPSYNC:
                    if (_animationMode == CurrentMode.CUSTOM)
                    {
                        var baseMotionName = _animationAssetManager.VmdList[_vmdIndex.Value];
                        var syncMotionName = _animationAssetManager.VmdSyncList[index];
                        FileReadAndWriteUtility.SaveMotionFacialPair(baseMotionName, syncMotionName);
                        // BaseVMDのまま更新したい
                        EvaluateAnimationIndex(0);
                    }
                    break;
            }
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        public void OnClickManualBook()
        {
            _bookService.ChangeOpenClose();
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        public void OnUpdateActorCount()
        {
            _textMeshs[2].text = $"{_playableBinderService.StageActorCount.Value}/{SystemInfo.MaxFieldChara}";
        }

        /// <summary>
        /// 該当するジャンプListを表示する
        /// </summary>
        /// <param name="btn"></param>
        void OpenJumplist(Button_Base btn)
        {
            if (_interactable == false) return;

            if (!_menuManager.jumpList.gameObject.activeSelf) _menuManager.jumpList.gameObject.SetActive(true);

            if (btn == _btnJumpList[0])
            {
                _menuManager.jumpList.SetActorAsync(_currentActorMode == CurrentMode.PRESET).Forget();
            }
            else if (btn == _btnJumpList[1])
            {
                _menuManager.jumpList.SetAnimeAsync(_animationMode == CurrentMode.PRESET).Forget();
            }
            else if (btn == _btnJumpList[2])
            {
                _menuManager.jumpList.SerAdditionalFacialAsync().Forget();
            }
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void OnClickSwitchChara(Button_Base btn)
        {
            if (_interactable == false) return;

            if (_switchChara[0] == btn)
            {
                _currentActorMode = CurrentMode.PRESET;
                _switchChara[0].isEnable = true;
                _switchChara[1].isEnable = false;
            }
            else
            {
                _currentActorMode = CurrentMode.CUSTOM;
                _switchChara[0].isEnable = false;
                _switchChara[1].isEnable = true;
            }
            _menuManager.jumpList.Close();
            EvaluateActorIndex(0);
        }

        void OnClickSwitchAnime(Button_Base btn)
        {
            if (_interactable == false) return;

            if (_switchAnime[0] == btn)
            {
                _animationMode = CurrentMode.PRESET;
                _switchAnime[0].isEnable = true;
                _switchAnime[1].isEnable = false;
            }
            else
            {
                _animationMode = CurrentMode.CUSTOM;
                _switchAnime[0].isEnable = false;
                _switchAnime[1].isEnable = true;
            }
            _menuManager.jumpList.Close();
            EvaluateAnimationIndex(0);
        }

        void OnMoveIndexActor(Button_Base btn)
        {
            if (_interactable == false) return;

            for (int i = 0; i < _btnChara.Length; i++)
            {
                if (_btnChara[i] != btn) continue;
                var moveIndex = i == 0 ? -1 : 1;
                EvaluateActorIndex(moveIndex);
                return;
            }
        }

        void OnMoveIndexAnimation(Button_Base btn)
        {
            if (_interactable == false) return;

            for (int i = 0; i < _btnAnime.Length; i++)
            {
                if (_btnAnime[i] != btn) continue;
                var moveIndex = i == 0 ? -1 : 1;
                EvaluateAnimationIndex(moveIndex);
                return;
            }
        }

        public void OnVRMLoadFrame()
        {
            var userMessage = MenuConstants.LoadVRM;
            _textMeshs[0].SetAutoSizedText(userMessage, 0.25f, 40);
        }

        /// <summary>
        /// 何れかのサムネボタンクリック時
        /// NOTE: サムネ表示中はCustomタブのIndex0扱い
        /// </summary>
        public void OnClickThumbnail()
        {
            _pleasePushText.gameObject.SetActive(false);
            var maxIndex = _actorEntityManagerService.NumRegisteredVRM - 1;
            var moveIndex = maxIndex - _vrmIndex.Value;
            EvaluateActorIndex(moveIndex);
        }

        void EvaluateActorIndex(int moveIndex)
        {
            _pleasePushText.gameObject.SetActive(false);//非表示で初期化しておく
            _vrmLoadFailureText.gameObject.SetActive(false);//非表示で初期化しておく
            _actorMaxText.gameObject.SetActive(false);

            if (SystemInfo.MaxFieldChara <= _playableBinderService.StageActorCount.Value)
            {
                _actorMaxText.gameObject.SetActive(true);
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                return;
            }

            if (_currentActorMode == CurrentMode.PRESET)
            {
                var pendingIndex = _fbxIndex.Value + moveIndex;
                if (pendingIndex < 0) pendingIndex = _actorEntityManagerService.NumRegisteredFBX - 1;
                else if (_actorEntityManagerService.NumRegisteredFBX <= pendingIndex) pendingIndex = 0;

                _loadingAnimation.gameObject.SetActive(true);

                if (_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(false);

                _fbxIndex.SetValueAndForceNotify(pendingIndex);
            }
            else if (_currentActorMode == CurrentMode.CUSTOM)
            {
                var pendingIndex = _vrmIndex.Value + moveIndex;
                if (pendingIndex < 0) pendingIndex = _actorEntityManagerService.NumRegisteredVRM - 1;
                else if (_actorEntityManagerService.NumRegisteredVRM <= pendingIndex) pendingIndex = 0;

                // 0はサムネページ
                if (pendingIndex == 0) _pleasePushText.gameObject.SetActive(true);
                else _loadingAnimation.gameObject.SetActive(true);
                if (!_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(true);

                _vrmIndex.SetValueAndForceNotify(pendingIndex);
            }
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        /// <summary>
        /// Timelineにバインド完了した直後を想定
        /// </summary>
        /// <param name="actorEntity"></param>
        public void OnBindingNewActor(ActorEntity actorEntity)
        {
            _loadingAnimation.gameObject.SetActive(false);
            UpdateActorInfo(actorEntity);
            EvaluateAnimationIndex(0, false);//生成後にAnimation反映
        }

        void UpdateActorInfo(ActorEntity actorEntity)
        {
            if (actorEntity == null) return;

            var actorName = actorEntity.CharaInfoData.viewName;
            _textMeshs[0].SetAutoSizedText(actorName, 0.25f, 40);

            _actorHeigthText.text = $"{actorEntity.Height.ToString("0.00")} m";
            _actorBoneCountText.text = $"{actorEntity.BonesCount} bones";
            _actorMaterialsCountText.text = $"{actorEntity.MaterialsCount} mat";
            _actorPolygonsText.text = $"▲{actorEntity.Polygons}";

            actorEntity.LookAtService.SetHeadWeight(_sliderHeadLook.Value);
            actorEntity.LookAtService.SetEyeWeight(_sliderEyeLook.Value);

            //モーフボタン初期化
            if (actorEntity.CharaInfoData.ActorType == ActorType.FBX)
            {
                if (_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(false);
            }
            else
            {
                if (!_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(true);
                _btnFacialActive.isEnable = true;
                _btnLipSyncActive.isEnable = true;
            }
        }

        /// <summary>
        /// indexが範囲内に収まっているか評価する
        /// 問題なければUIやクリック音に反映
        /// </summary>
        void EvaluateAnimationIndex(int moveIndex, bool isPlaySE = true)
        {
            if (_animationMode == CurrentMode.PRESET)
            {
                var pendingIndex = _clipIndex.Value + moveIndex;
                if (pendingIndex < 0) pendingIndex = _presetResourceData.DanceInfoData.Count - 1;
                else if (_presetResourceData.DanceInfoData.Count <= pendingIndex) pendingIndex = 0;
                _clipIndex.SetValueAndForceNotify(pendingIndex);
            }
            else if (_animationMode == CurrentMode.CUSTOM)
            {
                var pendingIndex = _vmdIndex.Value + moveIndex;
                if (pendingIndex < 0) pendingIndex = _animationAssetManager.VmdList.Count - 1;
                else if (_animationAssetManager.VmdList.Count <= pendingIndex) pendingIndex = 0;
                _vmdIndex.SetValueAndForceNotify(pendingIndex);
            }
            if (isPlaySE) _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        public void OnBindingNewAnimation()
        {
            UpdateAnimationInfo();
        }

        void UpdateAnimationInfo()
        {
            if (_animationMode == CurrentMode.PRESET)
            {
                var data = _presetResourceData.DanceInfoData[_clipIndex.Value];
                var baseMotionName = _isReverse.Value ? data.ViewName + " R" : data.ViewName;
                _textMeshs[1].SetAutoSizedText(baseMotionName, 0.25f, 40);

                //反転ボタン
                if (!_switchReverse.gameObject.activeSelf) _switchReverse.gameObject.SetActive(true);
                _sliderOffset.Value = 0;
                _textMeshs[3].text = $"{_sliderOffset.Value:0000}";
                if (_vmdAnchor.gameObject.activeSelf) _vmdAnchor.gameObject.SetActive(false);
            }
            else if (_animationMode == CurrentMode.CUSTOM)
            {
                if (_animationAssetManager.VmdList == null || _animationAssetManager.VmdList.Count <= 0)
                {
                    var noneMessage = TimelineConstants.NoCustomDanceMessage;
                    _textMeshs[1].SetAutoSizedText(noneMessage, 0.25f, 40);
                    return;
                }

                var baseMotionName = _animationAssetManager.VmdList[_vmdIndex.Value];
                _textMeshs[1].SetAutoSizedText(baseMotionName, 0.25f, 40);
                //反転ボタン
                if (_switchReverse.gameObject.activeSelf) _switchReverse.gameObject.SetActive(false);
                _sliderOffset.Value = FileReadAndWriteUtility.GetMotionOffset[baseMotionName];
                _textMeshs[3].text = $"{_sliderOffset.Value:0000}";
                if (!_vmdAnchor.gameObject.activeSelf) _vmdAnchor.gameObject.SetActive(true);
                var syncFileName = FileReadAndWriteUtility.TryGetSyncFileName(baseMotionName);
                if (string.IsNullOrEmpty(syncFileName)) syncFileName = TimelineConstants.NoCustomFacialSyncMessage;
                _textMeshs[4].SetAutoSizedText(syncFileName, 0.25f, 40);
            }
        }

        void OnClickVMDOffset(Button_Base btn)
        {
            if (_interactable == false) return;

            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (_btnOffset[i] == btn)
                {
                    var moveIndex = i == 0 ? 1 : -1;
                    //オフセットを設定
                    _sliderOffset.Value += moveIndex;
                    var baseMotion = _textMeshs[1].name;
                    FileReadAndWriteUtility.SetMotionOffset(baseMotion, (int)_sliderOffset.Value);
                    _textMeshs[3].text = $"{_sliderOffset.Value:0000}";
                    _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                    break;
                }
            }
            FileReadAndWriteUtility.SaveMotionOffset();
        }

        void OnChangeReverseAnimation(Button_Base btn)
        {
            if (_interactable == false) return;

            if (_animationMode != CurrentMode.PRESET) return;

            _isReverse.Value = _switchReverse.isEnable;
            EvaluateAnimationIndex(0);
        }

        /// <summary>
        /// VRM設定用画面を開く TODO:作り直す
        /// </summary>
        /// <param name="btn"></param>
        void VRMSetting(Button_Base btn)
        {
            //var portalChara = _timeline.BindCharaMap[TimelineController.PORTAL_INDEX];
            //if (portalChara && portalChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            //{
            //    //コピーをVRM設定画面に渡す
            //    _vrmSelectUI.VRMEditing(portalChara);
            //    //マニュアル開始
            //    var dummy = new CancellationToken();
            //    _playableMusicService.ManualMode(dummy).Forget();
            //}
            //_menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// VRMキャラを削除
        /// </summary>
        void OnClickVRMDelete(Button_Base btn)
        {
            if (_interactable == false) return;

            //サムネページの時は無視
            if (_vrmIndex.Value == 0) return;

            _actorEntityManagerService.DeleteVRM(_vrmIndex.Value);
            EvaluateActorIndex(-1);

            UniTask.Void(async () =>
            {
                await UniTask.Delay(1000);
                _ = Resources.UnloadUnusedAssets();//明示的に消しておく
            });
        }

        void OnClickVRMMode(Button_Base btn)
        {
            FileReadAndWriteUtility.UserProfile.IsVRM10 = btn.isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        /// <summary>
        /// フィールド一掃
        /// </summary>
        void OnClickDeleteAllActors(Button_Base btn)
        {
            if (_interactable == false) return;

            var message = new AllActorOperationMessage(ActorState.FIELD, ActorCommand.DELETE);
            _allPublisher.Publish(message);

            _audioSourceService.PlayOneShot(AudioSE.ObjectDelete);
        }

        void OnClickFacialExpression(Button_Base btn)
        {
            if (_interactable == false) return;

            if (btn == _btnFacialActive)
            {
                if (!_actorEntityManagerService.TryGetCurrentInstaceID(out var instanceId)) return;
                var command = _btnFacialActive.isEnable ? ActorCommand.FACILSYNC_ENEBLE : ActorCommand.FACILSYNC_DISABLE;
                var message = new ActorOperationMessage(instanceId, command);
                _publisher.Publish(message);
            }
            else if (btn == _btnLipSyncActive)
            {
                if (!_actorEntityManagerService.TryGetCurrentInstaceID(out var instanceId)) return;
                var command = _btnLipSyncActive.isEnable ? ActorCommand.LIPSYNC_ENEBLE : ActorCommand.LIPSYNC_DISABLE;
                var message = new ActorOperationMessage(instanceId, command);
                _publisher.Publish(message);
            }
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void DebugInput()
        {
            if (_interactable == false) return;

            if (Input.GetKeyDown(KeyCode.I)) EvaluateActorIndex(1);
            else if (Input.GetKeyDown(KeyCode.U)) EvaluateActorIndex(-1);
            else if (Input.GetKeyDown(KeyCode.K)) EvaluateAnimationIndex(1);
            else if (Input.GetKeyDown(KeyCode.J)) EvaluateAnimationIndex(-1);
        }
    }
}