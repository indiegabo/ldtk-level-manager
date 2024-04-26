using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LDtkVaniaEditor
{
    public class LevelElement : VisualElement
    {
        private const string TemplateName = "LevelInspector";
        private const string SceneLabelName = "LDtkVaniaScene";
        private const string AddressableGroupName = "LDtkVaniaScenes";
        private const string AddressableSceneLabel = "LDtkSceneLevel";
        private const string SceneAddressPrefix = "LDtkSceneLevel";

        private MV_Level _level;

        private TextField _fieldIid;

        private Button _buttonIidCopy;
        private Button _buttonCreateScene;
        private Button _buttonDestroyScene;

        private PropertyField _fieldAsset;
        private PropertyField _fieldLDtkAsset;
        private TextField _fieldAssetPath;
        private TextField _fieldAddressableKey;
        private TextField _fieldSceneAddressableKey;
        private PropertyField _fieldScene;

        public LevelElement(MV_Level level)
        {
            _level = level;
            TemplateContainer container = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldIid = container.Q<TextField>("field-iid");
            _fieldIid.SetEnabled(false);

            _buttonIidCopy = container.Q<Button>("button-iid-copy");
            _buttonIidCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldIid.text)) return;
                _fieldIid.text.CopyToClipboard();
            };

            _fieldScene = container.Q<PropertyField>("field-scene");
            _fieldScene.SetEnabled(false);

            _buttonCreateScene = container.Q<Button>("button-create-scene");
            _buttonCreateScene.clicked += () =>
            {
                CreateScene();
                EvaluateSceneDisplay();
            };

            _buttonDestroyScene = container.Q<Button>("button-destroy-scene");
            _buttonDestroyScene.clicked += () =>
            {
                DestroyScene();
                EvaluateSceneDisplay();
            };

            _fieldAsset = container.Q<PropertyField>("field-asset");
            _fieldAsset.SetEnabled(false);

            _fieldLDtkAsset = container.Q<PropertyField>("field-ldtk-asset");
            _fieldLDtkAsset.SetEnabled(false);

            _fieldAssetPath = container.Q<TextField>("field-asset-path");
            _fieldAssetPath.SetEnabled(false);

            Button buttonAssetPathCopy = container.Q<Button>("button-asset-path-copy");
            buttonAssetPathCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldAssetPath.text)) return;
                _fieldAssetPath.text.CopyToClipboard();
            };

            _fieldAddressableKey = container.Q<TextField>("field-addressable-key");
            _fieldAddressableKey.SetEnabled(false);

            Button buttonAdreesableKeyCopy = container.Q<Button>("button-addressable-key-copy");
            buttonAdreesableKeyCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldAddressableKey.text)) return;
                _fieldAddressableKey.text.CopyToClipboard();
            };

            _fieldSceneAddressableKey = container.Q<TextField>("field-scene-addressable-key");
            _fieldSceneAddressableKey.SetEnabled(false);

            Button buttonSceneAdreesableKeyCopy = container.Q<Button>("button-scene-addressable-key-copy");
            buttonSceneAdreesableKeyCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldSceneAddressableKey.text)) return;
                _fieldSceneAddressableKey.text.CopyToClipboard();
            };

            EvaluateSceneDisplay();
            Add(container);
        }

        private void EvaluateSceneDisplay()
        {
            if (string.IsNullOrEmpty(_level.SceneAssetGuid))
            {
                _fieldScene.style.display = DisplayStyle.None;
                _buttonDestroyScene.style.display = DisplayStyle.None;

                _buttonCreateScene.style.display = DisplayStyle.Flex;
            }
            else
            {
                _fieldScene.style.display = DisplayStyle.Flex;
                _buttonDestroyScene.style.display = DisplayStyle.Flex;

                _buttonCreateScene.style.display = DisplayStyle.None;
            }
        }

        private void CreateScene()
        {
            if (TryScenePath(_level.SceneAssetGuid, out string existentScenePath))
            {
                MV_Logger.Error($"A scene for level <color=#FFFFFF>{_level.Name}</color> already exists. It can be found at <color=#FFFFFF>{existentScenePath}</color> .", _level);
                return;
            }

            if (!RequestPathForUser(_level.Name, out string path)) return;

            GameObject ldtkLevelObject = PrefabUtility.InstantiatePrefab(_level.Asset) as GameObject;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.MoveGameObjectToScene(ldtkLevelObject, scene);

            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            string addressableAddress = $"{SceneAddressPrefix}_{_level.Iid}";
            if (!sceneAsset.TrySetAsAddressable(addressableAddress, AddressableGroupName, AddressableSceneLabel))
            {
                MV_Logger.Error($"Could not set scene for level <color=#FFFFFF>{_level.Name}</color> as addressable. Please check the console for errors.", _level);
            }

            _level.Scene = SceneField.FromAsset(sceneAsset);
            _level.SceneAssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset));
            _level.SceneAddressableKey = addressableAddress;
            AssetDatabase.SetLabels(sceneAsset, new string[] { SceneLabelName });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void DestroyScene()
        {
            if (!TryScenePath(_level.SceneAssetGuid, out string scenePath))
            {
                MV_Logger.Error($"Could not find scene for level <color=#FFFFFF>{_level.Name}</color> . Did you create the scene through a LDtkVaniaProject inspector?", _level);
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Caution!",
                $"Destroy scene for level {_level.Name}? This is irreversible and might result in work loss!",
                "I understand. Go on.",
                "Cancel"
            );

            if (!confirmed) return;

            AssetDatabase.DeleteAsset(scenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _level.Scene = null;
            _level.SceneAssetGuid = null;
            _level.SceneAddressableKey = null;
        }

        private bool RequestPathForUser(string levelName, out string scenePath)
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

        private bool TryScenePath(string guid, out string path)
        {
            if (string.IsNullOrEmpty(guid)) { path = null; return false; }
            path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return false;
            return true;
        }
    }
}