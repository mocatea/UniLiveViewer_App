using System;
using UniRx;
using UnityEngine;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuOnEnableHandler : MonoBehaviour
    {
        public IObservable<Unit> OnEnableAsObservable => _stream;
        readonly Subject<Unit> _stream = new();

        void Start()
        {
            // 初回Enable→OnEnableは反応しないのでStartでカバー
            _stream.OnNext(Unit.Default);
        }

        void OnEnable()
        {
            _stream.OnNext(Unit.Default);
        }
    }
}