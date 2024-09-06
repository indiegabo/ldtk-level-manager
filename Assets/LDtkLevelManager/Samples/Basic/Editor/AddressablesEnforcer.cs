using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDtkLevelManager.Implementations.Basic
{
    public class AddressablesEnforcer
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        public static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene.name != "Universe") return;

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Logger.Error("AddressableAssetSettings not found. Please create it in Window/Asset Management/Addressables/Groups and then sync levels in the LDtkLevelManagerProject inspector.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets($"LDtkLevelManagerProject t:{nameof(Project)}");
            if (guids.Length == 0) return;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Project project = AssetDatabase.LoadAssetAtPath<Project>(path);

            if (project == null) return;

            project.ReSync();
            project.EvaluateWorldAreas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}