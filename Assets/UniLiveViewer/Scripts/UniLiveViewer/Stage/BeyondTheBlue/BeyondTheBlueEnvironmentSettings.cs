using System;
using UnityEngine;
using UnityEngine.Assertions;
using VContainer;

namespace UniLiveViewer.Stage.BeyondTheBlue
{
    public class BeyondTheBlueEnvironmentSettings : MonoBehaviour
    {
        public Transform[] PropSets => _propSets;
        [SerializeField] Transform[] _propSets;

        public Transform GodRay => _godRay;
        [SerializeField] Transform _godRay;

        public Renderer[] Waters => _waters;
        [SerializeField] Renderer[] _waters;

        void Awake()
        {
            Assert.IsNotNull(_propSets);
            Assert.IsNotNull(_godRay);
            Assert.IsNotNull(_waters);

            for (int i = 0; i < _propSets.Length; i++)
            {
                Assert.IsNotNull(_propSets[i]);
            }
            for (int i = 0; i < _waters.Length; i++)
            {
                Assert.IsNotNull(_waters[i]);
            }
        }

        void Start()
        {
            // EnvironmentLSはMenuLSの子であり、該当ページが開かれるまで初期化できない為
            // 苦渋の末ここで初期化している
            _godRay.gameObject.SetActive(true);

            var initIndex = 1;
            for(int i = 0;i< _propSets.Length; i++)
            {
                _propSets[i].gameObject.SetActive(i == initIndex);
            }
        }
    }
}