using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace LDtkVania
{
    [System.Serializable]
    public class MV_LevelScene
    {
        #region Inspector

        [SerializeField] private string _assetGuid;
        [SerializeField] private string _addressableKey;

        public string AssetGuid { get => _assetGuid; set => _assetGuid = value; }
        public string AddressableKey { get => _addressableKey; set => _addressableKey = value; }

        #endregion

        #region  Unity Editor
#if UNITY_EDITOR
        public static readonly string SceneAddressPrefix = "LDtkSceneLevel";
        public static readonly string AddressableGroupName = "LDtkVaniaScenes";
        public static readonly string AddressableSceneLabel = "LDtkSceneLevel";
        public static readonly string SceneLabelName = "LDtkVaniaScene";

        public static bool CreateSceneForLevel(MV_Level level, out MV_LevelScene levelScene)
        {
            if (level.HasScene && TryScenePath(level.Scene.AssetGuid, out string existentScenePath))
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

            levelScene = new MV_LevelScene()
            {
                AssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset)),
                AddressableKey = addressableAddress
            };

            AssetDatabase.SetLabels(sceneAsset, new string[] { SceneLabelName });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        public static bool DestroySceneForLevel(MV_Level level)
        {
            if (!level.HasScene) return true;

            if (!TryScenePath(level.Scene.AssetGuid, out string scenePath))
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

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                return true;
            }

            List<string> labels = AssetDatabase.GetLabels(sceneAsset).ToList();
            labels.Add("LDtkSceneDeletion");

            AssetDatabase.SetLabels(sceneAsset, labels.ToArray());
            EditorUtility.SetDirty(sceneAsset);
            AssetDatabase.SaveAssetIfDirty(sceneAsset);

            if (!AssetDatabase.DeleteAsset(scenePath))
            {
                MV_Logger.Error($"Could not delete scene for level <color=#FFFFFF>{level.Name}</color> . Please check the console for errors.", level);
                return false;
            }

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