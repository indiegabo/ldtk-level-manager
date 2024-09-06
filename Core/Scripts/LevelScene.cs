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
    /// <summary>
    /// Represents a scene for a level. Useful for levels where you need to compose the level
    /// with stuf you only have available in the Unity Editor and would not be able to compose
    /// in the LDtk app.
    /// </summary>
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


        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Creates a scene for the given level and returns it as a LevelScene.
        /// If a scene already exists for the level, this method will return false and not 
        /// create a new scene.
        /// </summary>
        /// <param name="level">The level to create a scene for.</param>
        /// <param name="levelScene">The created LevelScene, if any.</param>
        /// <returns>True if the scene was created, false otherwise.</returns>
        public static bool CreateSceneForLevel(LevelInfo level, out LevelScene levelScene)
        {
            if (level.WrappedInScene && TryScenePath(level.SceneInfo.AssetGuid, out string existentScenePath))
            {
                Logger.Error(
                    $"A scene for level <color=#FFFFFF>{level.name}</color> already exists. "
                    + $"It can be found at <color=#FFFFFF>{existentScenePath}</color> .",
                    level
                );
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

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Destroys a scene associated with a level. <br/><br/>
        /// If the level has a scene, this method will delete the scene asset and remove the scene from the level information.
        /// </summary>
        /// <param name="level">The level to remove the scene from.</param>
        /// <param name="requestConfirmation">Whether or not to request confirmation from the user before destroying the scene.</param>
        /// <returns>True if the scene was successfully destroyed, false otherwise.</returns>
        public static bool DestroySceneForLevel(LevelInfo level, bool requestConfirmation = true)
        {
            if (!level.WrappedInScene) return true;

            if (!TryScenePath(level.SceneInfo.AssetGuid, out string scenePath))
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
                Logger.Error(
                    $"Could not delete scene for level <color=#FFFFFF>{level.name}</color> . "
                    + "Please check the console for errors.",
                    level
                );
                return false;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Enforces a scene to be addressable. If the scene does not exist, it will be removed from the level information.
        /// </summary>
        /// <param name="level">The level to enforce the scene for.</param>
        public static void EnforceSceneAddressable(LevelInfo level)
        {
            if (!level.WrappedInScene) return;

            if (!TryScenePath(level.SceneInfo.AssetGuid, out string path))
            {
                level.ClearScene();
                return;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            string addressableAddress = $"{AddressableSceneLabel}_{level.Iid}";
            if (!sceneAsset.TrySetAsAddressable(
                addressableAddress,
                AddressableGroupName,
                AddressableSceneLabel
            ))
            {
                Logger.Error(
                    $"Could not set scene for level <color=#FFFFFF>{level.name}</color> as addressable. "
                    + "Please check the console for errors.",
                    level
                );
            }

            string[] labels = AssetDatabase.GetLabels(sceneAsset);
            if (!labels.Contains(SceneLabelName))
            {
                AssetDatabase.SetLabels(sceneAsset, labels.Append(SceneLabelName).ToArray());
            }
        }


        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Regenerates the level object associated with the given level by opening the scene,
        /// removing the LDtk component and re-adding it.
        /// If the level does not have a scene, the method will do nothing and return.
        /// If the scene does not exist, it will be removed from the level information.
        /// </summary>
        /// <param name="level">The level for which the level object should be regenerated.</param>
        public static void RegenerateLevelObject(LevelInfo level)
        {
            if (!level.WrappedInScene) return;

            if (!TryScenePath(level.SceneInfo.AssetGuid, out string path))
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

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Attempts to get the path of a scene asset from a GUID.
        /// </summary>
        /// <param name="guid">The GUID of the scene asset.</param>
        /// <param name="path">The path of the scene asset, or null if the GUID is invalid.</param>
        /// <returns>True if the GUID is valid and the path is not null, false otherwise.</returns>
        public static bool TryScenePath(string guid, out string path)
        {
            if (string.IsNullOrEmpty(guid)) { path = null; return false; }
            path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return false;
            return true;
        }

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Requests a path from the user for a scene, given a level name.
        /// The method will return false if the user cancels the operation.
        /// Otherwise, it will return true and the scene path will be set to
        /// the given level name, with the correct extension and directory.
        /// </summary>
        /// <param name="levelName">The name of the level.</param>
        /// <param name="scenePath">The path of the scene, or null if the operation is canceled.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
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