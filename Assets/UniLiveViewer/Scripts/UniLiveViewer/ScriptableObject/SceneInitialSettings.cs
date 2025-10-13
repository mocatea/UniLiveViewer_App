using System;
using System.Collections.Generic;
using UniLiveViewer.SceneLoader;
using UnityEngine;

namespace UniLiveViewer.SO
{
    [CreateAssetMenu(menuName = "MyGame/SceneInitialSettings", fileName = "SceneInitialSettings")]
    public class SceneInitialSettings : ScriptableObject
    {
        public IReadOnlyList<SceneSettingsPair> Settings => _settings;
        public SceneInitialData GetSettingData(SceneType sceneType)
        {
            foreach (var setting in _settings)
            {
                if (setting.sceneType == sceneType)
                {
                    return setting.data;
                }
            }
            Debug.LogError("該当シーンデータ無し");
            return null;
        }

        [SerializeField] List<SceneSettingsPair> _settings;
    }

    [Serializable]
    public struct SceneSettingsPair
    {
        public SceneType sceneType;
        public SceneInitialData data;
    }
}

