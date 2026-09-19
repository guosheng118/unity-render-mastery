using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderLab.Editor
{
    public class GenshinCharacterGui : ShaderGUI
    {
        const int Width = 256;
        const int Height = 20;
        const int PixelsPerRow = 2;

        bool _editNight;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var material = materialEditor.target as Material;
            MaterialProperty rampMap = FindProperty("_RampMap", properties, false);

            if (rampMap != null && material != null)
                DrawRampEditor(materialEditor, material, rampMap);
            else if (rampMap == null)
                EditorGUILayout.HelpBox("这个 Shader 没有 _RampMap（脸材质不用 Ramp）。", MessageType.Info);

            foreach (MaterialProperty prop in properties)
            {
                if (prop.name == "_RampMap")
                    continue;
                if ((prop.propertyFlags & ShaderPropertyFlags.HideInInspector) != 0)
                    continue;
                materialEditor.ShaderProperty(prop, prop.displayName);
            }

            foreach (var target in materialEditor.targets)
            {
                var mat = target as Material;
                if (mat == null || !mat.HasProperty("_EnableOutline"))
                    continue;
                bool on = mat.GetFloat("_EnableOutline") > 0.5f;
                // URP 按 LightMode 抽 Pass，名字要用 SRPDefaultUnlit，光写 Outline 不会关
                mat.SetShaderPassEnabled("SRPDefaultUnlit", on);
                mat.SetShaderPassEnabled("Outline", on);
            }

            materialEditor.RenderQueueField();
        }

        void DrawRampEditor(MaterialEditor materialEditor, Material material, MaterialProperty rampMap)
        {
            EditorGUILayout.LabelField("Ramp", EditorStyles.boldLabel);
            materialEditor.TexturePropertySingleLine(new GUIContent("当前采样的图"), rampMap);
            EditorGUILayout.HelpBox(
                "上面槽里的图才是 Shader 在采的。点「创建」后，改色条会重新烘一张写进这个槽。",
                MessageType.Info);

            GenshinRampData data = FindRampData(material);
            if (data == null)
            {
                if (GUILayout.Button("创建可编辑 Ramp（写进材质子资源）"))
                {
                    EditorApplication.delayCall += () => CreateRampAssets(material, rampMap);
                }
                return;
            }

            data.Ensure();

            _editNight = EditorGUILayout.Popup("编辑", _editNight ? 1 : 0, new[] { "Day", "Night" }) == 1;
            Gradient[] rows = _editNight ? data.night : data.day;

            Undo.RecordObject(data, "Edit Ramp Gradient");
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < GenshinRampData.RowsPerMode; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(18));
                rows[i] = EditorGUILayout.GradientField(rows[i]);
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
                Bake(material, rampMap, data);

            EditorGUILayout.Space(4);
        }

        static GenshinRampData FindRampData(Material material)
        {
            string path = AssetDatabase.GetAssetPath(material);
            if (string.IsNullOrEmpty(path))
                return null;
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<GenshinRampData>().FirstOrDefault();
        }

        static Texture2D FindRampTex(Material material)
        {
            string path = AssetDatabase.GetAssetPath(material);
            if (string.IsNullOrEmpty(path))
                return null;
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Texture2D>()
                .FirstOrDefault(t => t.name == "RampTex");
        }

        static Texture2D NewRampTex()
        {
            return new Texture2D(Width, Height, TextureFormat.RGBA32, false, false)
            {
                name = "RampTex",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
                anisoLevel = 0,
                hideFlags = HideFlags.None
            };
        }

        static void CreateRampAssets(Material material, MaterialProperty rampMap)
        {
            string path = AssetDatabase.GetAssetPath(material);
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Ramp", "请先把材质保存成 Project 里的 .mat 资源，再创建 Ramp。", "OK");
                return;
            }

            if (FindRampData(material) != null)
                return;

            var data = ScriptableObject.CreateInstance<GenshinRampData>();
            data.name = "RampData";
            data.Ensure();
            AssetDatabase.AddObjectToAsset(data, material);

            var tex = NewRampTex();
            AssetDatabase.AddObjectToAsset(tex, material);
            AssetDatabase.SaveAssets();

            data = FindRampData(material);
            if (data != null)
                Bake(material, rampMap, data);
        }

        static void Bake(Material material, MaterialProperty rampMap, GenshinRampData data)
        {
            data.Ensure();

            Texture2D tex = FindRampTex(material);
            if (tex == null || !tex.isReadable)
            {
                if (tex != null)
                    AssetDatabase.RemoveObjectFromAsset(tex);

                tex = NewRampTex();
                string path = AssetDatabase.GetAssetPath(material);
                if (!string.IsNullOrEmpty(path))
                    AssetDatabase.AddObjectToAsset(tex, material);
            }

            if (tex.width != Width || tex.height != Height)
                tex.Reinitialize(Width, Height, TextureFormat.RGBA32, false);

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

            var pixels = new Color[Width * Height];
            BakeMode(pixels, data.day, 0);
            BakeMode(pixels, data.night, 1);
            tex.SetPixels(pixels);
            tex.Apply(false, false);

            rampMap.textureValue = tex;
            material.SetTexture("_RampMap", tex);
            EditorUtility.SetDirty(tex);
            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(material);
        }

        static void BakeMode(Color[] pixels, Gradient[] rows, int night)
        {
            for (int row = 0; row < GenshinRampData.RowsPerMode; row++)
            {
                Gradient g = rows[row] ?? new Gradient();
                int atlasFromTop = night * GenshinRampData.RowsPerMode + row;
                int y0 = (9 - atlasFromTop) * PixelsPerRow;
                for (int x = 0; x < Width; x++)
                {
                    Color c = g.Evaluate(x / 255f);
                    for (int dy = 0; dy < PixelsPerRow; dy++)
                        pixels[(y0 + dy) * Width + x] = c;
                }
            }
        }
    }
}
