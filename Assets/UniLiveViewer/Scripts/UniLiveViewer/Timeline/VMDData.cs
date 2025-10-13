using System.Collections.Generic;
using UniLiveViewer.External.UnityVMDReader;
using VContainer;

namespace UniLiveViewer.Timeline
{
    public class VMDData
    {
        public IReadOnlyDictionary<string, VMD> LoadedMap => _loadedMap;
        Dictionary<string, VMD> _loadedMap = new();
        int _currentIndex;

        readonly AnimationAssetManager _animationAssetManager;

        [Inject]
        public VMDData(AnimationAssetManager animationAssetManager)
        {
            _animationAssetManager = animationAssetManager;
        }

        /// <summary>
        /// 新規登録
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="vmd"></param>
        public void Add(string fileName, VMD vmd)
        {
            if (_loadedMap.ContainsKey(fileName)) return;
            _loadedMap[fileName] = vmd;
        }

        public void UpdateCurrent(int index)
        {
            _currentIndex = index;
        }

        public VMD TryGetCurrentVMD()
        {
            if (_animationAssetManager.VmdList == null || _animationAssetManager.VmdList.Count <= 0)
            {
                return null;
            }
            var viewName = _animationAssetManager.VmdList[_currentIndex];
            if (_loadedMap.ContainsKey(viewName)) return _loadedMap[viewName];
            return null;
        }

        /// <summary>
        /// ベース用
        /// </summary>
        /// <returns></returns>
        public string GetCurrentName()
        {
            if (_animationAssetManager == null || _animationAssetManager.VmdList.Count <= 0)
            {
                return string.Empty;
            }
            return _animationAssetManager.VmdList[_currentIndex];
        }

        /// <summary>
        /// 表情用
        /// None（一度設定したが解除したケース）の場合もtrueを返す
        /// </summary>
        /// <param name="syncFileName"></param>
        /// <returns></returns>
        public bool TryGetCurrentSyncName(out string syncFileName)
        {
            var vmdFileName = GetCurrentName();
            syncFileName = FileReadAndWriteUtility.TryGetSyncFileName(vmdFileName);
            if (syncFileName == null) return false;
            return true;
        }

        public VMD TryGetCurrentSyncVMD(string syncFileName)
        {
            if (_loadedMap.ContainsKey(syncFileName)) return _loadedMap[syncFileName];
            return null;
        }

        public void Clear()
        {
            _loadedMap.Clear();
        }
    }
}