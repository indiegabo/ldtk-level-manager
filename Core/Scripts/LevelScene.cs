using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using LDtkLevelManager.Utils;
using LDtkUnity;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace LDtkLevelManager
{
    [System.Serializable]
    public class LevelScene
    {
        #region Inspector

        [SerializeField] private string _assetGuid;
        [SerializeField] private string _addressableKey;

        public string AssetGuid { get => _assetGuid; set => _assetGuid = value; }
        public string AddressableKey { get => _addressableKey; set => _addressableKey = value; }

        #endregion

        #region  Unity Editor
#if UNITY_EDITOR
        public static readonly string AddressableGroupName = "LM_Scenes";
        public static readonly string AddressableSceneLabel = "LM_Scene";
        public static readonly string SceneLabelName = "LM_Scene";

        public static bool CreateSceneForLevel(LevelInfo level, out LevelScene levelScene)
        {
            if (level.HasScene && TryScenePath(level.Scene.AssetGuid, out string existentScenePath))
            {
                Logger.Error($"A scene for level <color=#FFFFFF>{level.name}</color> already exists. It can be found at <color=#FFFFFF>{existentScenePath}</color> .", level);
                levelScene = null;
                return false;
            }

            if (!RequestPathForUser(level.name, out string path))
            {
                levelScene = null;
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject ldtkLevelObject = PrefabUtility.InstantiatePrefab(level.Asset, scene) as GameObject;

            if (ldtkLevelObject.TryGetComponent(out SceneLevelSetuper setuper))
            {
                setuper.Setup(level);
            }

            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            string addressableAddress = $"{AddressableSceneLabel}_{level.Iid}";
            if (!sceneAsset.TrySetAsAddressable(addressableAddress, AddressableGroupName, AddressableSceneLabel))
            {
                Logger.Error($"Could not set scene for level <color=#FFFFFF>{level.name}</color> as addressable. Please check the console for errors.", level);
            }

            levelScene = new LevelScene()
            {
                AssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset)),
                AddressableKey = addressableAddress
            };

            AssetDatabase.SetLabels(sceneAsset, new string[] { SceneLabelName });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        public static bool DestroySceneForLevel(LevelInfo level, bool requestConfirmation = true)
        {
            if (!level.HasScene) return true;

            if (!TryScenePath(level.Scene.AssetGuid, out string scenePath))
            {
                Logger.Error($"Could not find scene for level <color=#FFFFFF>{level.name}</color> . Did you create the scene through a LDtkLevelManagerProject inspector?", level);
                return false;
            }

            if (requestConfirmation)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Caution!",
                    $"Destroy scene for level {level.name}? This is irreversible and might result in work loss!",
                    "I understand. Go on.",
                    "Cancel"
                );

                if (!confirmed) return false;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                return true;
            }

            AssetDatabase.SetLabels(sceneAsset, new string[0]);
            EditorUtility.SetDirty(sceneAsset);
            AssetDatabase.SaveAssetIfDirty(sceneAsset);

            if (!AssetDatabase.DeleteAsset(scenePath))
            {
                Logger.Error($"Could not delete scene for level <color=#FFFFFF>{level.name}</color> . Please check the console for errors.", level);
                return false;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        public static void EnforceSceneAddressable(LevelInfo level)
        {
            if (!level.HasScene) return;

            if (!TryScenePath(level.Scene.AssetGuid, out string path))
            {
                level.ClearScene();
                return;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            string addressableAddress = $"{AddressableSceneLabel}_{level.Iid}";
            if (!sceneAsset.TrySetAsAddressable(addressableAddress, AddressableGroupName, AddressableSceneLabel))
            {
                Logger.Error($"Could not set scene for level <color=#FFFFFF>{level.name}</color> as addressable. Please check the console for errors.", level);
            }

            string[] labels = AssetDatabase.GetLabels(sceneAsset);
            if (!labels.Contains(SceneLabelName))
            {
                AssetDatabase.SetLabels(sceneAsset, labels.Append(SceneLabelName).ToArray());
            }
        }

        /// <summary>
        /// Regenerates the level object in the scene for the given level. This is useful if you modified the level's prefab or its children.
        /// </summary>
        /// <param name="level">The level to regenerate the object for.</param>
        public static void RegenerateLevelObject(LevelInfo level)
        {
            if (!level.HasScene) return;

            if (!TryScenePath(level.Scene.AssetGuid, out string path))
            {
                level.ClearScene();
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            if (!scene.IsValid())
            {
                level.ClearScene();
                return;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out LDtkComponentLevel componentLevel))
                {
                    GameObject.DestroyImmediate(componentLevel.gameObject);
                    break;
                }
            }

            GameObject ldtkLevelObject = PrefabUtility.InstantiatePrefab(level.Asset, scene) as GameObject;
            if (ldtkLevelObject.TryGetComponent(out SceneLevelSetuper setuper))
            {
                setuper.Setup(level);
            }

            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);
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
                Logger.Error($"Scene path <color=#FFFFFF>{chosenPath}</color> is not in the project <color=#FFFFFF>{Application.dataPath}</color>.");
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