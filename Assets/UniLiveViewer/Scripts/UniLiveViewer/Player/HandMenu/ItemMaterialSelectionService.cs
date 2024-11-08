﻿using UniLiveViewer.Menu;
using UnityEngine;
using VContainer;
using UniRx;

namespace UniLiveViewer.Player.HandMenu
{
    public class ItemMaterialSelectionService
    {
        UniLiveViewer.HandMenu[] _handMenu = new UniLiveViewer.HandMenu[2];

        ItemMaterialSelector[] _itemMaterialSelector = new ItemMaterialSelector[2];

        bool _isSetupComplete;

        readonly PlayerHandMenuAnchorL _playerHandMenuAnchorL;
        readonly PlayerHandMenuAnchorR _playerHandMenuAnchorR;
        readonly PlayerHandMenuSettings _playerHandMenuSettings;
        readonly Transform _lookTarget;
        readonly RootAudioSourceService _audioSourceService;
        readonly SystemSettingsService _systemSettingsService;

        // TODO: Unity標準にする
        int _languageIndex;

        [Inject]
        public ItemMaterialSelectionService(
            PlayerHandMenuAnchorL playerHandMenuAnchorL,
            PlayerHandMenuAnchorR playerHandMenuAnchorR,
            PlayerHandMenuSettings playerHandMenuSettings,
            Camera camera,
            RootAudioSourceService audioSourceService,
            SystemSettingsService systemSettingsService)
        {
            _playerHandMenuAnchorL = playerHandMenuAnchorL;
            _playerHandMenuAnchorR = playerHandMenuAnchorR;
            _playerHandMenuSettings = playerHandMenuSettings;
            _lookTarget = camera.transform;
            _audioSourceService = audioSourceService;
            _systemSettingsService = systemSettingsService;
        }

        public void Setup()
        {
            _systemSettingsService.SystemLanguage
                .Select(x => x.ToResourceIndex())
                .Subscribe(x => _languageIndex = x);

            _handMenu[0] = new UniLiveViewer.HandMenu(
                GameObject.Instantiate(_playerHandMenuSettings.ItemMaterialSelection),
                _playerHandMenuAnchorL.transform);
            _handMenu[1] = new UniLiveViewer.HandMenu(
                GameObject.Instantiate(_playerHandMenuSettings.ItemMaterialSelection),
                _playerHandMenuAnchorR.transform);

            foreach (var handMenu in _handMenu)
            {
                handMenu.SetShow(false);
            }

            _isSetupComplete = true;
        }

        public void ForceCloseMenu(int index)
        {
            if (_handMenu.Length <= index) return;
            _handMenu[index].SetShow(false);
        }

        public void ChangeShow(int index, bool isShow, DecorationItemInfo decorationItemInfo)
        {
            if (_handMenu.Length <= index) return;
            if(_handMenu[index].IsShow == isShow) return;

            _handMenu[index].SetShow(isShow);
            if (isShow)
            {
                _audioSourceService.PlayOneShot(AudioSE.MenuOpen);
                if (decorationItemInfo == null) return;
                InitItemMaterialSelector(index, decorationItemInfo);
            }
            else
            {
                _audioSourceService.PlayOneShot(AudioSE.MenuClose);
            }
        }

        void InitItemMaterialSelector(int index, DecorationItemInfo decorationItemInfo)
        {
            if (_handMenu.Length <= index) return;
            _handMenu[index].SetShow(true);
            var itemMaterialSelector = _handMenu[index].Instance.GetComponent<ItemMaterialSelector>();
            _itemMaterialSelector[index] = itemMaterialSelector;
            _itemMaterialSelector[index].Initialize(decorationItemInfo, _languageIndex);
        }

        /// <summary>
        /// 指定Currentからテクスチャを取得
        /// </summary>
        public void SetItemTexture(int index, int current)
        {
            if (_itemMaterialSelector.Length <= index) return;
            var result = _itemMaterialSelector[index].TrySetTexture(current);
            if (result) _audioSourceService.PlayOneShot(AudioSE.ChangeItemColor);
        }

        public void OnLateTick()
        {
            if (!_isSetupComplete) return;

            foreach (var handMenu in _handMenu)
            {
                handMenu.UpdateLookat(_lookTarget);
            }
        }

        public bool IsShowAny()
        {
            foreach (var handMenu in _handMenu)
            {
                if (handMenu.IsShow) return true;
            }
            return false;
        }
    }

}