using UnityEditor;
using UnityEditor.SceneManagement;
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

            string[] guids = AssetDatabase.FindAssets($"LDtkLevelManagerProject t:{nameof(Project)}");
            if (guids.Length == 0) return;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Project project = AssetDatabase.LoadAssetAtPath<Project>(path);

            if (project == null) return;

            project.SyncLevels();
            project.EvaluateWorldAreas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}