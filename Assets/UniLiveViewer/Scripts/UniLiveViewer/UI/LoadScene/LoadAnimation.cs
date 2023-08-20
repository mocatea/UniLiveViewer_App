﻿using UnityEngine;

namespace UniLiveViewer
{
    public class LoadAnimation : MonoBehaviour
    {
        Animator _animator;
        int _current = 0;
        string[] _loadAnimeName = new string[2] { "isType01", "isType02" };
        [SerializeField] TextMesh _sceneName;

        void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        void OnDisable()
        {
            _animator.SetBool(_loadAnimeName[_current], false);
        }

        void OnEnable()
        {
            if (_sceneName) _sceneName.text = SceneManagerService.Current.VisualName;

            //ローディングアニメーションをランダム設定
            _current = Random.Range(0, 2);

            //ローディングアニメーション開始
            _animator.gameObject.SetActive(true);
            _animator.SetBool(_loadAnimeName[_current], true);
        }
    }
}