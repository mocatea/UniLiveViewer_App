using UnityEngine;
using UnityEngine.Assertions;

namespace UniLiveViewer.Actor.LookAt
{
    /// <summary>
    /// 実質プリセットであるFBX専用
    /// </summary>
    public class LookAtSettings : MonoBehaviour
    {
        /// <summary>
        /// 一部モデル用
        /// </summary>
        public int LookAtBlendShapeIndex => _lookAtBlendShapeIndex;
        [SerializeField] int _lookAtBlendShapeIndex;

        public Vector2 eyeAmplitude => _eyeAmplitude;
        [SerializeField] protected Vector2 _eyeAmplitude;

        public SkinnedMeshRenderer Face => _face;
        [SerializeField] SkinnedMeshRenderer _face;

        void Awake()
        {
            // VRMはランタイムに取得する故
            //Assert.IsNotNull(_face);
        }
    }
}