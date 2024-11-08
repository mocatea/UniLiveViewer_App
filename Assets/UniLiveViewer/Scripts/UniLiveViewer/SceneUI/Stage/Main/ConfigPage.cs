﻿using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// TODO: StageMenuLifetimeScopeに整理
    /// </summary>
    public class ConfigPage : MonoBehaviour
    {
        [Header("＜シーン別＞")]
        [SerializeField] SceneMenuAnchor[] _sceneAnchor;
        Button_Base[] _stageEventButton = new Button_Base[5];
        [SerializeField] Transform[] _actionParentButton;

        [Header("＜KAGURALive専用＞")]
        [SerializeField] SliderGrabController _fogSlider;

        [Header("＜ViewerScene専用＞")]
        [SerializeField] TextMesh[] _viewerTextMeshs = new TextMesh[4];

        [Header("＜Gym専用＞")]
        [SerializeField] TextMesh[] _gymTextMeshs = new TextMesh[1];

        public IObservable<int> StageLightIndexAsObservable => _stageLightIndex;
        readonly Subject<int> _stageLightIndex = new();
        public IObservable<bool> StageLightIsWhiteAsObservable => _stageLightIsWhite;
        readonly Subject<bool> _stageLightIsWhite = new();

        Material _matMirrore;//LiveScene用
        BackGroundController _backGroundCon;
        RootAudioSourceService _audioSourceService;
        CancellationToken _cancellation;

        [Inject]
        public void Construct(
            RootAudioSourceService audioSourceService)
        {
            _audioSourceService = audioSourceService;
        }

        public void OnStart()
        {
            _cancellation = this.GetCancellationTokenOnDestroy();
            _fogSlider.ValueAsObservable
                .Subscribe(x => RenderSettings.fogDensity = x).AddTo(this);
        }
        void OnEnable()
        {
            Init().Forget();
        }

        void Start()
        {
            // Title分を除外で-1
            var current = (int)SceneChangeService.GetSceneType - 1;

            //シーン応じて有効化を切り替える
            for (int i = 0; i < _sceneAnchor.Length; i++)
            {
                if (i == current)
                {
                    if (!_sceneAnchor[i].gameObject.activeSelf) _sceneAnchor[i].gameObject.SetActive(true);
                }
                else if (_sceneAnchor[i].gameObject.activeSelf)
                {
                    _sceneAnchor[i].gameObject.SetActive(false);
                }
            }

            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 5; i++)
                {
                    _stageEventButton[i] = _sceneAnchor[current].transform.GetChild(i).GetComponent<Button_Base>();
                }

                _actionParentButton = new Transform[5];

                _actionParentButton[0] = GameObject.FindGameObjectWithTag("Particle").transform;
                _actionParentButton[1] = GameObject.FindGameObjectWithTag("LaserGun").transform;
                _actionParentButton[2] = GameObject.FindGameObjectWithTag("FloorMirror").transform;
                _actionParentButton[3] = GameObject.FindGameObjectWithTag("SonicBoom").transform;
                _actionParentButton[4] = GameObject.FindGameObjectWithTag("ManualUI").transform;

                _matMirrore = _actionParentButton[2].GetComponent<MeshRenderer>().material;

                _actionParentButton[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_particle);
                _actionParentButton[1].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_laser);
                _matMirrore.SetFloat("_Smoothness", FileReadAndWriteUtility.UserProfile.scene_crs_reflection ? 1 : 0);
                _actionParentButton[3].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_sonic);
                _actionParentButton[4].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_manual);
            }
            else if (SceneChangeService.GetSceneType == SceneType.KAGURA_LIVE)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 3; i++)
                {
                    _stageEventButton[i] = _sceneAnchor[current].transform.GetChild(i).GetComponent<Button_Base>();
                }

                _actionParentButton = new Transform[3];

                _actionParentButton[0] = GameObject.FindGameObjectWithTag("Particle").transform;
                _actionParentButton[1] = GameObject.FindGameObjectWithTag("ReflectionProbe").transform;
                _actionParentButton[2] = GameObject.FindGameObjectWithTag("WaterAnchor").transform;

                _actionParentButton[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_particle);
                _actionParentButton[1].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_sea);
                _actionParentButton[2].transform.GetChild(0).gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_reflection);
            }
            else if (SceneChangeService.GetSceneType == SceneType.VIEWER)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 1; i++)
                {
                    _stageEventButton[i] = _sceneAnchor[current].transform.GetChild(i).GetComponent<Button_Base>();
                }

                _actionParentButton = new Transform[1];

                _actionParentButton[0] = GameObject.FindGameObjectWithTag("FloorLED").transform;
                _backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();

                _actionParentButton[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_view_led);
            }
            else if (SceneChangeService.GetSceneType == SceneType.GYMNASIUM)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 2; i++)
                {
                    _stageEventButton[i] = _sceneAnchor[current].transform.GetChild(i).GetComponent<Button_Base>();
                }
            }
            else if (SceneChangeService.GetSceneType == SceneType.FANTASY_VILLAGE)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 1; i++)
                {
                    _stageEventButton[i] = _sceneAnchor[current].transform.GetChild(i).GetComponent<Button_Base>();
                }
                _actionParentButton = new Transform[1];
                _actionParentButton[0] = GameObject.FindGameObjectWithTag("MainLight").transform;
            }

            _fogSlider.Value = 0.02f;
        }

        async UniTaskVoid Init()
        {
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);//一応

            var type = SceneChangeService.GetSceneType;
            if (type == SceneType.CANDY_LIVE)
            {
                //各種有効化状態にボタンを合わせる
                _stageEventButton[0].isEnable = _actionParentButton[0].gameObject.activeSelf;
                _stageEventButton[1].isEnable = _actionParentButton[1].gameObject.activeSelf;
                _stageEventButton[3].isEnable = _actionParentButton[3].gameObject.activeSelf;
                _stageEventButton[4].isEnable = _actionParentButton[4].gameObject.activeSelf;

                //反射状態に合わせる
                _stageEventButton[2].isEnable = (_matMirrore.GetFloat("_Smoothness") == 1.0f ? true : false);
            }
            else if (type == SceneType.KAGURA_LIVE)
            {
                //各種有効化状態にボタンを合わせる
                _stageEventButton[0].isEnable = _actionParentButton[0].gameObject.activeSelf;
                _stageEventButton[1].isEnable = _actionParentButton[1].gameObject.activeSelf;
                _stageEventButton[2].isEnable = _actionParentButton[2].transform.GetChild(0).gameObject.activeSelf;
            }
            else if (type == SceneType.VIEWER)
            {
                //各種有効化状態にボタンを合わせる
                _stageEventButton[0].isEnable = _actionParentButton[0].gameObject.activeSelf;
            }
            else if (type == SceneType.GYMNASIUM)
            {
                //各種有効化状態にボタンを合わせる
                _stageEventButton[0].isEnable = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;

                Click_Setting_Gym(0);
            }
            else if (type == SceneType.FANTASY_VILLAGE)
            {
                _stageEventButton[0].isEnable = _actionParentButton[0].gameObject.activeSelf;
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_Live(int i)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            bool result = _stageEventButton[i].isEnable;
            switch (i)
            {
                //パーティクル
                case 0:
                    if (_actionParentButton[0])
                    {
                        _actionParentButton[0].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_crs_particle = result;
                    }
                    break;
                //レーザーガン
                case 1:
                    if (_actionParentButton[1])
                    {
                        _actionParentButton[1].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_crs_laser = result;
                    }
                    break;
                //ミラー
                case 2:
                    if (_matMirrore)
                    {
                        _matMirrore.SetFloat("_Smoothness", (result == true ? 1 : 0));
                        FileReadAndWriteUtility.UserProfile.scene_crs_reflection = result;
                    }
                    break;
                //ソニックブーム
                case 3:
                    if (_actionParentButton[3])
                    {
                        _actionParentButton[3].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_crs_sonic = result;
                    }
                    break;
                //マニュアル
                case 4:
                    if (_actionParentButton[4])
                    {
                        _actionParentButton[4].gameObject.SetActive(result);
                    }
                    break;
            }

            //保存する
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_KAGURA(int i)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            bool result = _stageEventButton[i].isEnable;
            switch (i)
            {
                //パーティクル
                case 0:
                    if (_actionParentButton[0])
                    {
                        _actionParentButton[0].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_kagura_particle = result;
                    }
                    break;
                //リフレクション
                case 1:
                    if (_actionParentButton[1])
                    {
                        _actionParentButton[1].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_kagura_reflection = result;
                    }
                    break;
                //海切り替えテスト
                case 2:
                    if (_actionParentButton[2])
                    {
                        if (_actionParentButton[2].GetChild(0).gameObject.activeSelf)
                        {
                            _actionParentButton[2].GetChild(0).gameObject.SetActive(false);
                            _actionParentButton[2].GetChild(1).gameObject.SetActive(true);
                        }
                        else if (_actionParentButton[2].GetChild(1).gameObject.activeSelf)
                        {
                            _actionParentButton[2].GetChild(1).gameObject.SetActive(false);
                            _actionParentButton[2].GetChild(0).gameObject.SetActive(true);
                        }
                        FileReadAndWriteUtility.UserProfile.scene_kagura_sea = result;
                    }
                    break;
            }
            //まとめて保存する
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_Viewer(int i)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            var result = _stageEventButton[i].isEnable;
            switch (i)
            {
                //LED
                case 0:
                    if (_actionParentButton[0])
                    {
                        _actionParentButton[0].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_view_led = result;

                        //保存する
                        FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
                    }
                    break;
            }
        }

        public void Click_ChangeEffect(int i)
        {
            if (!_backGroundCon) return;
            string str;

            switch (i)
            {
                case 0:
                    _backGroundCon.SetCubemap(-1, out str);
                    _viewerTextMeshs[0].text = "SkyBox_" + str;
                    break;
                case 1:
                    _backGroundCon.SetCubemap(1, out str);
                    _viewerTextMeshs[0].text = "SkyBox_" + str;
                    break;
                case 2:
                    _backGroundCon.SetWormHole(-1, out str);
                    _viewerTextMeshs[1].text = "WormHole_" + str;
                    break;
                case 3:
                    _backGroundCon.SetWormHole(1, out str);
                    _viewerTextMeshs[1].text = "WormHole_" + str;
                    break;
                case 4:
                    _backGroundCon.SetParticle(-1, out str);
                    _viewerTextMeshs[2].text = "Particle_" + str;
                    break;
                case 5:
                    _backGroundCon.SetParticle(1, out str);
                    _viewerTextMeshs[2].text = "Particle_" + str;
                    break;
            }

            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        // 雑
        int _lightIndex = StageEnums.StageLightDefaultIndex;
        public void Click_ChangeStageLight(int i)
        {
            var moveIndex = i == 0 ? -1 : 1;
            _lightIndex += moveIndex;
            var max = Enum.GetValues(typeof(StageEnums.StageLight)).Length;
            if (max <= _lightIndex) _lightIndex = 0;
            else if (_lightIndex < 0) _lightIndex = max - 1;

            _stageLightIndex.OnNext(_lightIndex);
            _gymTextMeshs[0].text = $"SpotLight_{Enum.GetName(typeof(StageEnums.StageLight), _lightIndex)}";
            _audioSourceService.PlayOneShot(AudioSE.SpotlightSwitch);
        }

        public void Click_Setting_Gym(int i)
        {
            switch (i)
            {
                case 0:
                    _stageLightIsWhite.OnNext(_stageEventButton[0].isEnable);
                    FileReadAndWriteUtility.UserProfile.scene_gym_whitelight = _stageEventButton[0].isEnable;
                    break;
            }

            //保存する
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        public void Click_Setting_FantasyVillage(int i)
        {
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);

            bool result = _stageEventButton[i].isEnable;
            switch (i)
            {
                //ライト
                case 0:
                    if (_actionParentButton[0])
                    {
                        _actionParentButton[0].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_fv_light = result;
                    }
                    break;
            }

            //保存する
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) Click_Setting_Viewer(2);//ワームホール
            if (Input.GetKeyDown(KeyCode.K)) Click_Setting_Viewer(3);//パーティクル
        }
    }
}