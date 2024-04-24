using LDtkVania;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    [CustomEditor(typeof(MV_Level))]
    public class MV_LevelInspector : Editor
    {
        public VisualTreeAsset _inspectorXML;

        private TextField _fieldIid;
        private Button _buttonIidCopy;
        private PropertyField _fieldAsset;
        private PropertyField _fieldLDtkAsset;
        private TextField _fieldAssetPath;
        private TextField _fieldAddressableKey;
        private PropertyField _fieldScenes;

        public override VisualElement CreateInspectorGUI()
        {

            // Create a new VisualElement to be the root of our Inspector UI.
            VisualElement inspectorElement = new();
            TemplateContainer template = _inspectorXML.Instantiate();

            _fieldIid = template.Q<TextField>("field-iid");
            _fieldIid.SetEnabled(false);

            _buttonIidCopy = template.Q<Button>("button-iid-copy");
            _buttonIidCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldIid.text)) return;
                _fieldIid.text.CopyToClipboard();
            };

            _fieldAsset = template.Q<PropertyField>("field-asset");
            _fieldAsset.SetEnabled(false);

            _fieldLDtkAsset = template.Q<PropertyField>("field-ldtk-asset");
            _fieldLDtkAsset.SetEnabled(false);

            _fieldAssetPath = template.Q<TextField>("field-asset-path");
            _fieldAssetPath.SetEnabled(false);

            Button buttonAssetPathCopy = template.Q<Button>("button-asset-path-copy");
            buttonAssetPathCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldAssetPath.text)) return;
                _fieldAssetPath.text.CopyToClipboard();
            };

            _fieldAddressableKey = template.Q<TextField>("field-addressable-key");
            _fieldAddressableKey.SetEnabled(false);

            Button buttonAdreesableKeyCopy = template.Q<Button>("button-addressable-key-copy");
            buttonAdreesableKeyCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_fieldAddressableKey.text)) return;
                _fieldAddressableKey.text.CopyToClipboard();
            };

            _fieldScenes = template.Q<PropertyField>("field-scenes");

            // Load from default reference.
            inspectorElement.Add(template);

            // Return the finished Inspector UI.
            return inspectorElement;
        }
    }
}
