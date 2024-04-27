using LDtkUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

namespace LDtkVania
{
    [System.Serializable]
    public class MV_LevelScene
    {
        #region Inspector

        [SerializeField] private SceneField _scene;
        [SerializeField] private string _sceneAssetGuid;
        [SerializeField] private string _sceneAddressableKey;

        public SceneField Scene { get => _scene; set => _scene = value; }
        public string SceneAssetGuid { get => _sceneAssetGuid; set => _sceneAssetGuid = value; }
        public string SceneAddressableKey { get => _sceneAddressableKey; set => _sceneAddressableKey = value; }

        #endregion

        #region  Unity Editor
#if UNITY_EDITOR

        private static readonly string SceneAddressPrefix = "LDtkSceneLevel";
        private static readonly string AddressableGroupName = "LDtkVaniaScenes";
        private static readonly string AddressableSceneLabel = "LDtkSceneLevel";
        private static readonly string SceneLabelName = "LDtkVaniaScene";

        public static bool CreateSceneForLevel(MV_Level level, out MV_LevelScene levelScene)
        {
            if (level.HasScene && TryScenePath(level.Scene.SceneAssetGuid, out string existentScenePath))
            {
                MV_Logger.Error($"A scene for level <color=#FFFFFF>{level.Name}</color> already exists. It can be found at <color=#FFFFFF>{existentScenePath}</color> .", level);
                levelScene = null;
                return false;
            }

            if (!RequestPathForUser(level.Name, out string path))
            {
                levelScene = null;
                return false;
            }

            GameObject ldtkLevelObject = PrefabUtility.InstantiatePrefab(level.Asset) as GameObject;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.MoveGameObjectToScene(ldtkLevelObject, scene);

            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            string addressableAddress = $"{SceneAddressPrefix}_{level.Iid}";
            if (!sceneAsset.TrySetAsAddressable(addressableAddress, AddressableGroupName, AddressableSceneLabel))
            {
                MV_Logger.Error($"Could not set scene for level <color=#FFFFFF>{level.Name}</color> as addressable. Please check the console for errors.", level);
            }

            levelScene = new()
            {
                Scene = SceneField.FromAsset(sceneAsset),
                SceneAssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset)),
                SceneAddressableKey = addressableAddress
            };

            AssetDatabase.SetLabels(sceneAsset, new string[] { SceneLabelName });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        public static bool DestroySceneForLevel(MV_Level level)
        {
            if (!level.HasScene) return false;

            if (!TryScenePath(level.Scene.SceneAssetGuid, out string scenePath))
            {
                MV_Logger.Error($"Could not find scene for level <color=#FFFFFF>{level.Name}</color> . Did you create the scene through a LDtkVaniaProject inspector?", level);
                return false;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Caution!",
                $"Destroy scene for level {level.Name}? This is irreversible and might result in work loss!",
                "I understand. Go on.",
                "Cancel"
            );

            if (!confirmed) return false;

            AssetDatabase.DeleteAsset(scenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        public static bool TryScenePath(string guid, out string path)
        {
            if (string.IsNullOrEmpty(guid)) { path = null; return false; }
            path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return false;
            return true;
        }

        private static bool RequestPathForUser(string levelName, out string scenePath)
        {
            string chosenPath = EditorUtility.OpenFolderPanel("Select a folder", Application.dataPath, "");

            if (string.IsNullOrEmpty(chosenPath)) { scenePath = null; return false; }

            if (!chosenPath.StartsWith(Application.dataPath))
            {
                MV_Logger.Error($"Scene path <color=#FFFFFF>{chosenPath}</color> is not in the project <color=#FFFFFF>{Application.dataPath}</color>.");
                scenePath = null;
                return false;
            }

            string strippedPath = chosenPath.Replace(Application.dataPath, "Assets");
            scenePath = $"{strippedPath}/{levelName}.unity";
            return true;
        }
#endif
        #endregion
    }
}