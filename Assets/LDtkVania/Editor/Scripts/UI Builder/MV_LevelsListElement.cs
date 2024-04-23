using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using LDtkUnity;
using UnityEditor;

namespace LDtkVaniaEditor
{
    public class MV_LevelsListElement : VisualElement
    {
        #region Fields

        private MV_Level _level;
        private Foldout _foldoutMain;
        private TextField _fieldIid;
        private Button _buttonIidCopy;
        private ObjectField _fieldAsset;
        private ObjectField _fieldLDtkAsset;
        private TextField _fieldAssetPath;
        private TextField _fieldAddressableKey;
        private ListView _fieldScenes;

        #endregion

        #region Properties

        public MV_Level Level
        {
            get => _level;
            set
            {
                _level = value;

                _foldoutMain.text = _level.Name;
                _fieldIid.value = _level.Iid;
                _fieldAsset.value = _level.Asset;
                _fieldLDtkAsset.value = _level.LevelFile;
                _fieldAssetPath.value = _level.AssetPath;
                _fieldAddressableKey.value = _level.AddressableKey;
            }
        }

        #endregion

        #region Constructors

        public MV_LevelsListElement(VisualTreeAsset tree)
        {
            TemplateContainer template = tree.Instantiate();
            _foldoutMain = template.Q<Foldout>("foldout-main");
            Toggle toggle = _foldoutMain.Q<Toggle>();
            toggle.style.marginLeft = 0;

            _fieldIid = template.Q<TextField>("field-iid");
            _fieldIid.SetEnabled(false);
            _fieldIid.style.marginRight = 0;
            _fieldIid.SetEnabled(false);

            _buttonIidCopy = template.Q<Button>("button-iid-copy");
            _buttonIidCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_level.Iid)) return;
                _level.Iid.CopyToClipboard();
            };

            _fieldAsset = template.Q<ObjectField>("field-asset");
            _fieldAsset.SetEnabled(false);
            _fieldAsset.objectType = typeof(Object);

            _fieldLDtkAsset = template.Q<ObjectField>("field-ldtk-asset");
            _fieldLDtkAsset.SetEnabled(false);

            _fieldAssetPath = template.Q<TextField>("field-asset-path");
            _fieldAssetPath.SetEnabled(false);

            Button buttonAssetPathCopy = template.Q<Button>("button-asset-path-copy");
            buttonAssetPathCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_level.AssetPath)) return;
                _level.AssetPath.CopyToClipboard();
            };

            _fieldAddressableKey = template.Q<TextField>("field-addressable-key");
            _fieldAddressableKey.SetEnabled(false);

            Button buttonAdreesableKeyCopy = template.Q<Button>("button-addressable-key-copy");
            buttonAdreesableKeyCopy.clicked += () =>
            {
                if (string.IsNullOrEmpty(_level.AddressableKey)) return;
                _level.AddressableKey.CopyToClipboard();
            };

            _fieldScenes = template.Q<ListView>("field-scenes");

            _foldoutMain.SetValueWithoutNotify(false);
            Add(template);
        }

        #endregion
    }
}