using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using UnityEditor;

namespace LDtkVaniaEditor
{
    public class LevelElement : VisualElement
    {
        private const string TemplateName = "LevelInspector";

        private MV_Level _level;

        TemplateContainer _containerMain;
        LevelSceneElement _levelSceneElement;

        private TextField _fieldIid;

        private Button _buttonCompletlyRemove;
        private Button _buttonIidCopy;
        private Button _buttonCreateScene;
        private Button _buttonDestroyScene;

        private VisualElement _containerLeftBehind;
        private TextField _fieldWorld;
        private TextField _fieldArea;
        private VisualElement _containerSceneElement;
        private PropertyField _fieldAsset;
        private PropertyField _fieldLDtkAsset;
        private TextField _fieldAssetPath;
        private TextField _fieldAddressableKey;

        public LevelElement(MV_Level level)
        {
            _level = level;
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _containerLeftBehind = _containerMain.Q<VisualElement>("container-left-behind");
            _containerLeftBehind.style.display = _level.LeftBehind ? DisplayStyle.Flex : DisplayStyle.None;

            _buttonCompletlyRemove = _containerMain.Q<Button>("button-completly-remove");
            _buttonCompletlyRemove.clicked += OnCompletlyRemove;

            _fieldIid = _containerMain.Q<TextField>("field-iid");
            _fieldIid.SetEnabled(false);

            _fieldWorld = _containerMain.Q<TextField>("field-world");
            _fieldWorld.SetEnabled(false);
            _fieldWorld.style.display = string.IsNullOrEmpty(_level.WorldName) ? DisplayStyle.None : DisplayStyle.Flex;

            _fieldArea = _containerMain.Q<TextField>("field-area");
            _fieldArea.SetEnabled(false);
            _fieldArea.style.display = string.IsNullOrEmpty(_level.AreaName) ? DisplayStyle.None : DisplayStyle.Flex;

            _buttonIidCopy = _containerMain.Q<Button>("button-iid-copy");
            _buttonIidCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldIid.text)) return;
                _fieldIid.text.CopyToClipboard();
            };

            _levelSceneElement = new LevelSceneElement();
            _containerSceneElement = _containerMain.Q<VisualElement>("container-scene-element");
            _containerSceneElement.Add(_levelSceneElement);
            if (_level.HasScene)
            {
                _levelSceneElement.LevelScene = _level.Scene;
            }

            _buttonCreateScene = _containerMain.Q<Button>("button-create-scene");
            _buttonCreateScene.SetEnabled(!_level.LeftBehind);
            _buttonCreateScene.clicked += () =>
            {
                if (MV_LevelScene.CreateSceneForLevel(_level, out MV_LevelScene levelScene))
                {
                    _level.SetScene(levelScene);
                    _levelSceneElement.LevelScene = levelScene;
                    EditorUtility.SetDirty(_level);
                    AssetDatabase.SaveAssetIfDirty(_level);
                }
                EvaluateSceneDisplay();
            };

            _buttonDestroyScene = _containerMain.Q<Button>("button-destroy-scene");
            _buttonDestroyScene.clicked += DestroyScene;

            _fieldAsset = _containerMain.Q<PropertyField>("field-asset");
            _fieldAsset.SetEnabled(false);

            _fieldLDtkAsset = _containerMain.Q<PropertyField>("field-ldtk-asset");
            _fieldLDtkAsset.SetEnabled(false);

            _fieldAssetPath = _containerMain.Q<TextField>("field-asset-path");
            _fieldAssetPath.SetEnabled(false);

            Button buttonAssetPathCopy = _containerMain.Q<Button>("button-asset-path-copy");
            buttonAssetPathCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldAssetPath.text)) return;
                _fieldAssetPath.text.CopyToClipboard();
            };

            _fieldAddressableKey = _containerMain.Q<TextField>("field-address");
            _fieldAddressableKey.SetEnabled(false);

            Button buttonAdreesableKeyCopy = _containerMain.Q<Button>("button-address-copy");
            buttonAdreesableKeyCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldAddressableKey.text)) return;
                _fieldAddressableKey.text.CopyToClipboard();
            };

            EvaluateSceneDisplay();
            Add(_containerMain);
        }

        private void EvaluateSceneDisplay()
        {
            if (_level.HasScene)
            {
                _containerSceneElement.style.display = DisplayStyle.Flex;
                _buttonDestroyScene.style.display = DisplayStyle.Flex;

                _buttonCreateScene.style.display = DisplayStyle.None;
            }
            else
            {
                _containerSceneElement.style.display = DisplayStyle.None;
                _buttonDestroyScene.style.display = DisplayStyle.None;

                _buttonCreateScene.style.display = DisplayStyle.Flex;
            }
        }

        private void OnCompletlyRemove()
        {
            _level.Project.RemoveLevel(_level.Iid);
            EditorUtility.SetDirty(_level.Project);
            AssetDatabase.SaveAssetIfDirty(_level.Project);
            AssetDatabase.Refresh();
        }

        private void DestroyScene()
        {
            if (MV_LevelScene.DestroySceneForLevel(_level))
            {
                _level.SetScene(null);
                _levelSceneElement.LevelScene = null;
                EditorUtility.SetDirty(_level);
                AssetDatabase.SaveAssetIfDirty(_level);
            }

            EvaluateSceneDisplay();
        }
    }
}