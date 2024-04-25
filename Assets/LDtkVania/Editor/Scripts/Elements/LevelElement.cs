using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using LDtkUnity;
using UnityEditor;

namespace LDtkVaniaEditor
{
    public class LevelElement : VisualElement
    {
        private const string TemplateName = "LevelInspector";

        private TextField _fieldIid;
        private Button _buttonIidCopy;
        private PropertyField _fieldAsset;
        private PropertyField _fieldLDtkAsset;
        private TextField _fieldAssetPath;
        private TextField _fieldAddressableKey;
        private PropertyField _fieldScenes;

        public LevelElement()
        {
            TemplateContainer container = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldIid = container.Q<TextField>("field-iid");
            _fieldIid.SetEnabled(false);

            _buttonIidCopy = container.Q<Button>("button-iid-copy");
            _buttonIidCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldIid.text)) return;
                _fieldIid.text.CopyToClipboard();
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

            _fieldScenes = container.Q<PropertyField>("field-scenes");
            Add(container);
        }
    }
}