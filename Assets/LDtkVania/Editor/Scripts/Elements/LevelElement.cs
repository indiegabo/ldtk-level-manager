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
        private const string LDtkVaniaSceneName = "LDtkVaniaScene";

        private MV_Level _level;

        private TextField _fieldIid;

        private Button _buttonIidCopy;
        private Button _buttonAddScene;
        private Button _buttonRemoveScene;

        private PropertyField _fieldAsset;
        private PropertyField _fieldLDtkAsset;
        private TextField _fieldAssetPath;
        private TextField _fieldAddressableKey;
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

            _buttonAddScene = container.Q<Button>("button-add-scene");
            _buttonAddScene.clicked += () => CreateScene();

            _buttonRemoveScene = container.Q<Button>("button-remove-scene");
            _buttonRemoveScene.clicked += () => RemoveScene();

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

            _fieldScene = container.Q<PropertyField>("field-scenes");

            Add(container);
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
            _level.SceneAssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset));

            AssetDatabase.SetLabels(sceneAsset, new string[] { LDtkVaniaSceneName });
            _level.Scene = SceneField.FromAsset(sceneAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void RemoveScene()
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
        }

        private bool RequestPathForUser(string levelName, out string scenePath)
        {
            string chosenPath = EditorUtility.OpenFolderPanel("Select a folder", Application.dataPath, "");

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