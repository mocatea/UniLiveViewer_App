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

        public IObservable<SceneType> ChangeSceneAsObservable => _stream;
        readonly Subject<SceneType> _stream = new();

        void Awake()
        {
            Assert.IsNotNull(_textMaxActor);
            Assert.IsNotNull(_sceneButton);

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
            _textMaxActor[4].text = SystemInfo.GetMaxFieldActor(SceneType.FANTASY_VILLAGE).ToString();

            // Button_Base改修するまでの繋ぎ
            _sceneButton[0].onTrigger += (btn) => _stream.OnNext(SceneType.TITLE);
            _sceneButton[1].onTrigger += (btn) => _stream.OnNext(SceneType.CANDY_LIVE);
            _sceneButton[2].onTrigger += (btn) => _stream.OnNext(SceneType.KAGURA_LIVE);
            _sceneButton[3].onTrigger += (btn) => _stream.OnNext(SceneType.VIEWER);
            _sceneButton[4].onTrigger += (btn) => _stream.OnNext(SceneType.GYMNASIUM);
            _sceneButton[5].onTrigger += (btn) => _stream.OnNext(SceneType.FANTASY_VILLAGE);
        }
    }
}
