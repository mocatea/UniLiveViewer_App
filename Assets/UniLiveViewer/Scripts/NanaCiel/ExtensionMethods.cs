using System.Collections.Generic;
using UnityEngine;

namespace NanaCiel
{
    // 未整理汎用拡張メソッド
    public static class ExtensionMethods
    {
        /// <summary>
        /// 水平方向ベクトルを取得する
        /// </summary>
        public static Vector3 GetHorizontalDirection(this Vector3 originalVector)
        {
            return new Vector3(originalVector.x, 0.0f, originalVector.z);
        }

        /// <summary>
        /// 垂直方向ベクトルを取得する
        /// </summary>
        public static Vector3 GetVerticalDirection(this Vector3 originalVector)
        {
            return new Vector3(0.0f, originalVector.y, originalVector.z);
        }

        public static Vector3 RandomQuake(this Vector3 maxMagnitude)
        {
            maxMagnitude.x = Random.Range(-maxMagnitude.x, maxMagnitude.x);
            maxMagnitude.y = Random.Range(-maxMagnitude.y, maxMagnitude.y);
            maxMagnitude.z = Random.Range(-maxMagnitude.z, maxMagnitude.z);
            return maxMagnitude;
        }

        /// <summary>
        /// blendshapeが設定されているSkinnedMeshRendererのみ抽出
        /// </summary>
        public static IReadOnlyList<SkinnedMeshRenderer> GetMorphSkinnedMeshRenderer(this IReadOnlyList<SkinnedMeshRenderer> skinMeshs)
        {
            List<SkinnedMeshRenderer> result = new List<SkinnedMeshRenderer>();
            foreach (var skinMesh in skinMeshs)
            {
                if (skinMesh.sharedMesh.blendShapeCount > 0)
                {
                    result.Add(skinMesh);
                }
            }
            return result.ToArray();
        }

        public static T Also<T>(this T self, System.Action<T> action)
        {
            action(self);
            return self;
        }

        public static R Let<T, R>(this T self, System.Func<T, R> action)
        {
            return action(self);
        }

        /// <summary>
        /// TryGetComponentじゃ複数取れないので
        /// </summary>
        public static T[] TryGetComponents<T>(Transform transform) where T : Component
        {
            return transform.GetComponents<T>() ?? new T[0];
        }
    }
}