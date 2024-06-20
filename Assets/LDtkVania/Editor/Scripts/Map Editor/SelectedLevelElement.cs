using System;
using LDtkUnity;
using LDtkVania;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class SelectedLevelElement : VisualElement
    {
        private const string TemplateName = "SelectedLevel";

        private MapLevelElement _mapLevelElement;
        private LDtkUnity.Level _level;
        private LDtkVania.LevelInfo _levelInfo;

        private TemplateContainer _containerMain;

        private Label _labelName;
        private Button _buttonLoad;
        private Button _buttonUnload;

        public SelectedLevelElement(MapLevelElement mapLevelElement)
        {
            _mapLevelElement = mapLevelElement;
            _level = _mapLevelElement.Level;
            _levelInfo = _mapLevelElement.Info;

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _labelName = _containerMain.Q<Label>("label-name");
            _labelName.text = _levelInfo.Name;

            _buttonLoad = _containerMain.Q<Button>("button-load");
            _buttonLoad.clicked += ToggleLevel;

            _buttonUnload = _containerMain.Q<Button>("button-unload");
            _buttonUnload.clicked += ToggleLevel;

            _mapLevelElement.LoadedStatusChanged += OnMapElementLoadedStatusChanged;

            EvaluateLoadButtons();

            Add(_containerMain);
        }

        public void Dismiss()
        {
            _mapLevelElement.LoadedStatusChanged -= OnMapElementLoadedStatusChanged;
        }

        private void EvaluateLoadButtons()
        {
            if (_mapLevelElement.Loaded)
            {
                _buttonLoad.style.display = DisplayStyle.None;
                _buttonUnload.style.display = DisplayStyle.Flex;
            }
            else
            {
                _buttonLoad.style.display = DisplayStyle.Flex;
                _buttonUnload.style.display = DisplayStyle.None;
            }
        }

        private void ToggleLevel()
        {
            _mapLevelElement.ToggleLoaded();
            EvaluateLoadButtons();
        }

        private void OnMapElementLoadedStatusChanged(bool isLoaded)
        {
            EvaluateLoadButtons();
        }
    }
}