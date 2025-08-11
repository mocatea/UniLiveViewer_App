using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NanaCiel
{
    public static class MeshExtensionMethods
    {
        /// <summary>
        /// 総ボーン数をカウント
        /// </summary>
        public static int GetUniqueBoneCount(this GameObject go)
        {
            if (!go) return 0;

            var smrList = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            var uniqueBones = new HashSet<Transform>();
            foreach (var smr in smrList)
            {
                if (smr.bones == null) continue;
                foreach (var bone in smr.bones)
                {
                    if (bone != null) uniqueBones.Add(bone);
                }
            }
            return uniqueBones.Count;
        }

        /// <summary>
        /// ポリゴン数をカウント
        /// SkinnedMeshRenderer/MeshRendererを集計
        /// LODGroupがある場合は0のみ対象とする
        /// </summary>
        public static int GetPolygonCountRecursive(GameObject obj)
        {
            int count = 0;

            foreach (var lodGroup in obj.GetComponentsInChildren<LODGroup>(true))
            {
                var lods = lodGroup.GetLODs();
                if (lods.Length > 0) // LOD0が存在
                {
                    foreach (var r in lods[0].renderers)
                    {
                        count += CountRendererPolygons(r);
                    }
                }
            }

            // LODGroupに属さないメッシュも数える
            var lodRenderers = new System.Collections.Generic.HashSet<Renderer>();
            foreach (var lodGroup in obj.GetComponentsInChildren<LODGroup>(true))
            {
                foreach (var lod in lodGroup.GetLODs())
                {
                    foreach (var r in lod.renderers) lodRenderers.Add(r);
                }
            }

            foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
            {
                if (!lodRenderers.Contains(renderer))
                {
                    count += CountRendererPolygons(renderer);
                }
            }

            return count;
        }

        static int CountRendererPolygons(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer smr && smr.sharedMesh != null)
            {
                return smr.sharedMesh.triangles.Length / 3;
            }
            else if (renderer is MeshRenderer mr)
            {
                var mf = mr.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    return mf.sharedMesh.triangles.Length / 3;
                }
            }
            return 0;
        }

        /// <summary>
        /// マテリアル数をカウント
        /// </summary>
        public static int GetUniqueMaterialAssetCount(this GameObject root, bool includeInactive = true)
        {
            if (!root) return 0;

            var guidSet = new HashSet<string>();
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive);
            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (!m) continue;

                    var path = AssetDatabase.GetAssetPath(m);
                    if (!string.IsNullOrEmpty(path))
                    {
                        var guid = AssetDatabase.AssetPathToGUID(path);
                        if (!string.IsNullOrEmpty(guid)) guidSet.Add(guid);
                    }
                    else
                    {
                        // アセットでない(ランタイム生成)マテリアルはインスタンスIDで区別
                        guidSet.Add($"runtime:{m.GetInstanceID()}");
                    }
                }
            }
            return guidSet.Count;
        }
    }
}