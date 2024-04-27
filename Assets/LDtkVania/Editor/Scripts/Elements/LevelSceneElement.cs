using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.AI;
using UnityEditor.SceneManagement;

namespace LDtkVaniaEditor
{
    public delegate void DestroyRequestedEvent();
    public class LevelSceneElement : VisualElement
    {
        private const string TemplateName = "LevelSceneInspector";

        private MV_LevelScene _levelScene;

        private ObjectField _fieldAsset;
        private Button _buttonOpenScene;

        public MV_LevelScene LevelScene
        {
            get => _levelScene;
            set
            {
                _levelScene = value;

                if (_levelScene != null)
                {
                    _fieldAsset.SetValueWithoutNotify(_levelScene.Asset);
                }
                else
                {
                    _fieldAsset.SetValueWithoutNotify(null);
                }
            }
        }

        public LevelSceneElement()
        {
            TemplateContainer container = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldAsset = container.Q<ObjectField>("field-asset");
            _fieldAsset.SetEnabled(false);

            _buttonOpenScene = container.Q<Button>("button-open-scene");
            _buttonOpenScene.clicked += OnOpenSceneRequested;

            Add(container);
        }

        private void OnOpenSceneRequested()
        {
            if (_fieldAsset.value == null) return;
            string path = AssetDatabase.GetAssetPath(_fieldAsset.value);
            EditorSceneManager.OpenScene(path);
        }
    }
}