using System.Collections.Generic;
using UnityEngine;

namespace NanaCiel
{
    public static class AnimatorExtensionMethods
    {
        /// <summary>
        /// humanDescription基準姿勢(T/A)に戻す（恐らくEditor推奨）
        /// </summary>
        public static void ResetToReferencePose(this Animator animator)
        {
            if (!animator || !animator.avatar || !animator.avatar.isValid) return;

            Dictionary<string, Transform> _boneMap;
            _boneMap = new Dictionary<string, Transform>();
            foreach (var t in animator.GetComponentsInChildren<Transform>(true))
            {
                if (!_boneMap.ContainsKey(t.name)) _boneMap.Add(t.name, t);
            }

            var skeleton = animator.avatar.humanDescription.skeleton;
            for (int i = 0; i < skeleton.Length; i++)
            {
                var bone = skeleton[i];
                if (_boneMap.TryGetValue(bone.name, out var t))
                {
                    t.localPosition = bone.position;
                    t.localRotation = bone.rotation;
                    // t.localScale = bone.scale; // 必要なら
                }
            }
        }

        /// <summary>
        /// muscle基準姿勢(丸まった姿勢)に戻す
        /// </summary>
        public static void ResetToReferenceMusclePose(this Animator animator)
        {
            if (!animator || !animator.avatar || !animator.avatar.isValid || !animator.avatar.isHuman) return;

            var handler = new HumanPoseHandler(animator.avatar, animator.transform);
            var pose = new HumanPose();
            handler.GetHumanPose(ref pose);

            pose.muscles = new float[HumanTrait.MuscleCount];// すべて 0 = Avatar の基準姿勢
            //pose.bodyRotation = Quaternion.identity;// 必要なら
            handler.SetHumanPose(ref pose);
        }

        /// <summary>
        /// 厳密な身長を測定（下端＝animatorのY、上端＝全メッシュ頂点の最大ワールドY）。
        /// </summary>
        /// <param name="topWorldPoint">最上点ワールド座標も欲しい場合</param>
        public static float MeasureExactHeight(this Animator animator, out Vector3 topWorldPoint)
        {
            topWorldPoint = default;
            if (!animator) return 0f;

            var bottomY = animator.transform.position.y;
            var topY = float.NegativeInfinity;

            var bakedMesh = new Mesh();
            bakedMesh.MarkDynamic();

            var verts = new List<Vector3>(4096);

            // 子階層の全Renderer（非アクティブ含む）
            var renderers = animator.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r is SkinnedMeshRenderer smr)
                {
                    var sm = smr.sharedMesh;
                    if (!sm) continue;

                    // 現在ポーズで変形後メッシュをベイクして頂点評価
                    smr.BakeMesh(bakedMesh, useScale: true);
                    bakedMesh.GetVertices(verts);
                    var l2w = smr.localToWorldMatrix;

                    for (int i = 0; i < verts.Count; i++)
                    {
                        float y = (l2w.MultiplyPoint3x4(verts[i])).y;
                        if (y > topY)
                        {
                            topY = y;
                            topWorldPoint = new Vector3(
                                (l2w.MultiplyPoint3x4(verts[i])).x,
                                y,
                                (l2w.MultiplyPoint3x4(verts[i])).z
                            );
                        }
                    }
                }
                else if (r is MeshRenderer mr)
                {
                    var mf = mr.GetComponent<MeshFilter>();
                    if (!mf || !mf.sharedMesh) continue;

                    mf.sharedMesh.GetVertices(verts);
                    var l2w = mf.transform.localToWorldMatrix;

                    for (int i = 0; i < verts.Count; i++)
                    {
                        var y = l2w.MultiplyPoint3x4(verts[i]).y;
                        if (y > topY)
                        {
                            topY = y;
                            topWorldPoint = new Vector3(
                                (l2w.MultiplyPoint3x4(verts[i])).x,
                                y,
                                (l2w.MultiplyPoint3x4(verts[i])).z
                            );
                        }
                    }
                }
            }

            if (float.IsNegativeInfinity(topY)) return 0f; // メッシュなし
            return Mathf.Max(0f, topY - bottomY);
        }
    }
}