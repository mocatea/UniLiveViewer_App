﻿using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// EndlessBookが癖あるのでUnityLocalization使わず手動で切り替える
    /// </summary>
    public class BookService
    {
        GameObject _boolObj;

        readonly BookSetting _bookSetting;
        readonly BookAnchor _bookAnchor;

        [Inject]
        public BookService(BookSetting bookSetting, BookAnchor bookAnchor)
        {
            _bookSetting = bookSetting;
            _bookAnchor = bookAnchor;
        }

        public void Initialize(SystemLanguage systemLanguage)
        {
            if (_boolObj != null)
            {
                GameObject.Destroy(_boolObj);
                _boolObj = null;
            }

            var prefab = systemLanguage switch
            {
                SystemLanguage.English => _bookSetting.PrefabEN,
                SystemLanguage.Japanese => _bookSetting.PrefabJP,
                _ => null,
            };

            if (prefab == null) return;
            _boolObj = GameObject.Instantiate(prefab, _bookAnchor.transform);
        }

        public void ChangeOpenClose()
        {
            _bookAnchor.gameObject.SetActive(!_bookAnchor.gameObject.activeSelf);
        }
    }
}