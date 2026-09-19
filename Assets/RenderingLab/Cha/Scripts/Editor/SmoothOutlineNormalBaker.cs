using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RenderLab.Editor
{
    /// <summary>
    /// FBX 导入时把「按位置平均」的平滑法线写入 TANGENT.xyz。
    /// Unity GPU 蒙皮会变换 POSITION / NORMAL / TANGENT，不会变换 UV，
    /// 所以描边跟动作走必须用切线通道，不能用 UV2。
    /// 光照仍读 NORMAL（硬法线）；描边读 TANGENT。
    /// 仅处理 Assets/RenderingLab/Cha 下的模型。
    /// </summary>
    public class SmoothOutlineNormalPostprocessor : AssetPostprocessor
    {
        const float PosScale = 10000f;
        const string Folder = "Assets/RenderingLab/Cha";

        void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.Replace('\\', '/').StartsWith(Folder))
                return;

            var seen = new HashSet<Mesh>();
            foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.sharedMesh != null && seen.Add(smr.sharedMesh))
                    BakeSmoothNormalsToTangent(smr.sharedMesh);
            }

            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh != null && seen.Add(filter.sharedMesh))
                    BakeSmoothNormalsToTangent(filter.sharedMesh);
            }
        }

        [MenuItem("RenderingLab/Reimport Models (Smooth Outline Normals)")]
        static void ReimportSelected()
        {
            var paths = new HashSet<string>();
            foreach (Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (IsModel(path))
                    paths.Add(path);
            }

            foreach (GameObject go in Selection.gameObjects)
            {
                foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    TryAddMeshPath(smr.sharedMesh, paths);
                foreach (MeshFilter filter in go.GetComponentsInChildren<MeshFilter>(true))
                    TryAddMeshPath(filter.sharedMesh, paths);
            }

            if (paths.Count == 0)
            {
                EditorUtility.DisplayDialog("Smooth Outline Normals",
                    "请选中 Cha 目录下的 FBX，或场景里用该 FBX 的角色。", "OK");
                return;
            }

            foreach (string path in paths)
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        static void TryAddMeshPath(Mesh mesh, HashSet<string> paths)
        {
            if (mesh == null)
                return;
            string path = AssetDatabase.GetAssetPath(mesh);
            if (IsModel(path))
                paths.Add(path);
        }

        static bool IsModel(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            path = path.Replace('\\', '/');
            if (!path.StartsWith(Folder))
                return false;
            string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            return ext == ".fbx" || ext == ".obj" || ext == ".blend";
        }

        internal static void BakeSmoothNormalsToTangent(Mesh mesh)
        {
            Vector3[] positions = mesh.vertices;
            Vector3[] normals = mesh.normals;
            if (positions == null || normals == null || positions.Length != normals.Length)
                return;

            var accum = new Dictionary<Vector3Int, Vector3>(positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3Int key = ToKey(positions[i]);
                if (accum.TryGetValue(key, out Vector3 sum))
                    accum[key] = sum + normals[i];
                else
                    accum[key] = normals[i];
            }

            Vector4[] oldTangents = mesh.tangents;
            var tangents = new Vector4[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 n = accum[ToKey(positions[i])].normalized;
                if (n.sqrMagnitude < 1e-8f)
                    n = normals[i];
                float w = (oldTangents != null && oldTangents.Length == positions.Length)
                    ? oldTangents[i].w
                    : 1f;
                tangents[i] = new Vector4(n.x, n.y, n.z, w);
            }

            mesh.tangents = tangents;
        }

        static Vector3Int ToKey(Vector3 p)
        {
            return Vector3Int.RoundToInt(p * PosScale);
        }
    }
}
