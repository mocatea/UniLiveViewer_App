using Cysharp.Threading.Tasks;
using NanaCiel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniLiveViewer.Actor;
using UniLiveViewer.SO;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    //本当はスクロールビューにしたい、ボコボコボタン生成めちゃ高コスト
    public class JumpList : MonoBehaviour
    {
        const int MaxFontWidth = 40;
        const int MaxFontLength = 20;

        public enum TARGET
        {
            NULL,
            CHARA,
            ANIME,
            VMD_LIPSYNC,
            AUDIO,
        }

        public IObservable<(TARGET, int)> OnSelectAsObservable => _selectStream;
        Subject<(TARGET, int)> _selectStream = new();
        TARGET _target = TARGET.NULL;

        [SerializeField] Button_Base Button_BasePrefab;
        [SerializeField] Transform parentAnchor;
        PresetResourceData _presetResourceData;
        ActorEntityManagerService _actorEntityManagerService;
        AnimationAssetManager _animationAssetManager;
        AudioAssetManager _audioAssetManager;
        List<Button_Base> _btnList = new();
        AudioClipSettings _audioClipSettings;
        RootAudioSourceService _audioSourceService;

        [Inject]
        public void Construct(
            PresetResourceData presetResourceData,
            ActorEntityManagerService actorEntityManagerService,
            AnimationAssetManager animationAssetManager,
            AudioAssetManager audioAssetManager,
            AudioClipSettings audioClipSettings,
            RootAudioSourceService audioSourceService)
        {
            _presetResourceData = presetResourceData;
            _actorEntityManagerService = actorEntityManagerService;
            _animationAssetManager = animationAssetManager;
            _audioAssetManager = audioAssetManager;
            _audioClipSettings = audioClipSettings;
            _audioSourceService = audioSourceService;
            Close();
        }

        /// <summary>
        /// 必要に応じてボタンを追加生成
        /// </summary>
        public void IfNeededCreateButton(int needCount)
        {
            if (_btnList.Count >= needCount) return;

            const int MAXLINE = 20;//行数
            const float BETWEEN_ROWS = 0.38f;//列間
            const float BETWEEN_LINE = 0.026f;//行間
            float initX = 0, initY = 0;

            Button_Base btn;
            for (int i = _btnList.Count; i < needCount; i++)
            {
                initX = 0.07f + i / MAXLINE * BETWEEN_ROWS;
                initY = 0.21f - (i % MAXLINE * BETWEEN_LINE);

                btn = Instantiate(Button_BasePrefab);
                btn.onTrigger += OnClick;
                btn.transform.SetParent(parentAnchor);

                btn.transform.localRotation = Quaternion.identity;
                btn.transform.localScale = Vector3.one;

                //Zファイティング対策
                if ((initX / 3) % 2 == 0) btn.transform.localPosition = new Vector3(initX, initY, 0);
                else btn.transform.localPosition = new Vector3(initX, initY, -0.01f);

                _btnList.Add(btn);
                btn = null;
            }
        }

        /// <summary>
        /// アクター情報を設定する
        /// </summary>
        public async UniTask SetActorAsync(bool isPreset)
        {
            var viewNames = isPreset
                ? _actorEntityManagerService.FbxViewNames : _actorEntityManagerService.VRMViewNames;
            IfNeededCreateButton(viewNames.Length);

            for (int i = 0; i < _btnList.Count; i++)
            {
                if (i < viewNames.Length)
                {
                    if (viewNames[i] != null) _btnList[i].SetTextMesh(viewNames[i]);
                    else _btnList[i].SetTextMesh(MenuConstants.LoadVRM);

                    if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                }
                else if (viewNames.Length <= i)
                {
                    if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                }
            }
            _target = TARGET.CHARA;
            await UniTask.Delay(400, cancellationToken: this.GetCancellationTokenOnDestroy());
            _audioSourceService.PlayOneShot(AudioSE.SpringMenuItem);
        }

        /// <summary>
        /// アニメーション情報を設定する
        /// </summary>
        public async UniTask SetAnimeAsync(bool isPreset)
        {
            var danceInfoData = isPreset
                ? _presetResourceData.DanceInfoData.Select(x => x.ViewName).ToList() : _animationAssetManager.VmdList;
            IfNeededCreateButton(danceInfoData.Count);

            for (int i = 0; i < _btnList.Count; i++)
            {
                if (i < danceInfoData.Count)
                {
                    var abbreviatedName = danceInfoData[i].TruncateWithEllipsis(MaxFontWidth, MaxFontLength);
                    _btnList[i].SetTextMesh(abbreviatedName);
                    if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                }
                else if (danceInfoData.Count <= i)
                {
                    if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                }
            }
            _target = TARGET.ANIME;
            await UniTask.Delay(400, cancellationToken: this.GetCancellationTokenOnDestroy());
            _audioSourceService.PlayOneShot(AudioSE.SpringMenuItem);
        }

        /// <summary>
        /// 追加表情情報を設定する
        /// </summary>
        public async UniTask SerAdditionalFacialAsync()
        {
            var lipSyncNames = _animationAssetManager.VmdSyncList;
            IfNeededCreateButton(lipSyncNames.Count);

            for (int i = 0; i < _btnList.Count; i++)
            {
                if (i < lipSyncNames.Count)
                {
                    var abbreviatedName = lipSyncNames[i].TruncateWithEllipsis(MaxFontWidth, MaxFontLength);
                    _btnList[i].SetTextMesh(abbreviatedName);
                    if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                }
                else if (lipSyncNames.Count <= i)
                {
                    if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                }
            }
            _target = TARGET.VMD_LIPSYNC;
            await UniTask.Delay(400, cancellationToken: this.GetCancellationTokenOnDestroy());
            _audioSourceService.PlayOneShot(AudioSE.SpringMenuItem);
        }

        /// <summary>
        /// 楽曲情報を設定する
        /// </summary>
        public async UniTask SetAudioAsync(bool isPresetAudio)
        {
            if (isPresetAudio)
            {
                var count = _audioClipSettings.AudioBGM.Count;
                IfNeededCreateButton(count);

                for (int i = 0; i < _btnList.Count; i++)
                {
                    if (i < count)
                    {
                        var name = Path.GetFileName(_audioClipSettings.AudioBGM[i].name);
                        var abbreviatedName = name.TruncateWithEllipsis(MaxFontWidth, MaxFontLength);
                        _btnList[i].SetTextMesh(abbreviatedName);
                        if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //必要ならボタンを生成
                var count = _audioAssetManager.CustomAudios.Count;
                IfNeededCreateButton(count);

                for (int i = 0; i < _btnList.Count; i++)
                {
                    if (i < count)
                    {
                        var name = Path.GetFileName(_audioAssetManager.CustomAudios[i]);
                        var abbreviatedName = name.TruncateWithEllipsis(MaxFontWidth, MaxFontLength);
                        _btnList[i].SetTextMesh(abbreviatedName);
                        if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                    }
                }
            }
            _target = TARGET.AUDIO;
            await UniTask.Delay(400, cancellationToken: this.GetCancellationTokenOnDestroy());
            _audioSourceService.PlayOneShot(AudioSE.SpringMenuItem);
        }

        /// <summary>
        /// リスト内のいずれかのボタンがクリックされた
        /// </summary>
        void OnClick(Button_Base btn)
        {
            //ボタンを特定
            for (int i = 0; i < _btnList.Count; i++)
            {
                if (btn != _btnList[i]) continue;
                _selectStream.OnNext((_target, i));
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
                Debug.Log($"ジャンプボタンIndex:{i}");
                break;
            }
            gameObject.SetActive(false);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}