using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Interactions.Editor
{
    public static class WeatherMaterialFixer
    {
        [MenuItem("Alchemy/Fix Pink Weather Shaders")]
        [MenuItem("Alchemy/Fix Pink Weather Shaders")]
        public static void FixShaders()
        {
            string[] searchFolders = new[] { "Assets/RainMaker", "Assets/Realistic Hail Set" };
            string[] guids = AssetDatabase.FindAssets("t:Material", searchFolders);
            Debug.Log($"WeatherMaterialFixer: Found {guids.Length} materials in search folders.");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat != null)
                {
                    bool needsFix = mat.shader.name.Contains("Custom/Rain") || 
                                    mat.shader.name.Contains("Particles/Additive") || 
                                    mat.shader.name == "Hidden/InternalErrorShader" ||
                                    mat.shader.name == "Standard";
                    
                    if (needsFix)
                    {
                        mat.shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                        EditorUtility.SetDirty(mat);
                        count++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"WeatherMaterialFixer: Converted {count} materials to URP Particles/Unlit.");
        }
    }
}
