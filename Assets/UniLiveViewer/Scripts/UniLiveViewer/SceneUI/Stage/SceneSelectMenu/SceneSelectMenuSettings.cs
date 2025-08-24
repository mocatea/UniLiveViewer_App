using System;
using UniLiveViewer.SceneLoader;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Menu.SceneSelect
{
    public class SceneSelectMenuSettings : MonoBehaviour
    {
        [SerializeField] TextMesh[] _textMaxActor;
        [SerializeField] Button_Switch[] _sceneButton;
        [SerializeField] Button_Switch _tileSceneButton;

        public IObservable<SceneType> ChangeSceneAsObservable => _stream;
        readonly Subject<SceneType> _stream = new();

        void Awake()
        {
            Assert.IsNotNull(_textMaxActor);
            Assert.IsNotNull(_sceneButton);
            Assert.IsNotNull(_tileSceneButton);

            for (int i = 0; i < _textMaxActor.Length; i++)
            {
                Assert.IsNotNull(_textMaxActor[i]);
            }
            for (int i = 0; i < _sceneButton.Length; i++)
            {
                Assert.IsNotNull(_sceneButton[i]);
            }
        }

        void Start()
        {
            foreach (var button in _sceneButton)
            {
                button.isEnable = false;
            }

            _textMaxActor[0].text = SystemInfo.GetMaxFieldActor(SceneType.CANDY_LIVE).ToString();
            _textMaxActor[1].text = SystemInfo.GetMaxFieldActor(SceneType.KAGURA_LIVE).ToString();
            _textMaxActor[2].text = SystemInfo.GetMaxFieldActor(SceneType.VIEWER).ToString();
            _textMaxActor[3].text = SystemInfo.GetMaxFieldActor(SceneType.GYMNASIUM).ToString();
            _textMaxActor[4].text = SystemInfo.GetMaxFieldActor(SceneType.BEYOND_THE_BLUE).ToString();
            _textMaxActor[5].text = SystemInfo.GetMaxFieldActor(SceneType.SNOW_FIELD).ToString();
            _textMaxActor[6].text = SystemInfo.GetMaxFieldActor(SceneType.FANTASY_VILLAGE).ToString();

            _sceneButton[0].OnTriggerAsObservable().Subscribe(_ => _stream.OnNext(SceneType.CANDY_LIVE)).AddTo(this);
            _sceneButton[1].OnTriggerAsObservable().Subscribe(_ => _stream.OnNext(SceneType.KAGURA_LIVE)).AddTo(this);
            _sceneButton[2].OnTriggerAsObservable().Subscribe(_ => _stream.OnNext(SceneType.VIEWER)).AddTo(this);
            _sceneButton[3].OnTriggerAsObservable().Subscribe(_ => _stream.OnNext(SceneType.GYMNASIUM)).AddTo(this);
            _sceneButton[4].OnTriggerAsObservable().Subscribe(_ => _stream.OnNext(SceneType.BEYOND_THE_BLUE)).AddTo(this);
            _sceneButton[5].OnTriggerAsObservable().Subscribe(_ => _stream.OnNext(SceneType.SNOW_FIELD)).AddTo(this);
            _sceneButton[6].OnTriggerAsObservable().Subscribe(_ => _stream.OnNext(SceneType.FANTASY_VILLAGE)).AddTo(this);

            _tileSceneButton.OnTriggerAsObservable().Subscribe(_ => _stream.OnNext(SceneType.TITLE)).AddTo(this);
        }
    }
}
