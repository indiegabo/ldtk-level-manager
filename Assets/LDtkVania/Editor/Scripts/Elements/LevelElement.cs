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
                if (MV_LevelScene.CreateSceneForLevel(_level, out MV_LevelScene levelScene))
                {
                    _level.SetScene(levelScene);
                }

                EvaluateSceneDisplay();
            };

            _buttonDestroyScene = container.Q<Button>("button-destroy-scene");
            _buttonDestroyScene.clicked += () =>
            {
                MV_LevelScene.DestroySceneForLevel(_level);
                _level.SetScene(null);
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
            if (_level.HasScene)
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
    }
}