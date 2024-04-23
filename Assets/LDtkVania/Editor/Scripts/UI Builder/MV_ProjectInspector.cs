using System.Collections.Generic;
using LDtkVania;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    [CustomEditor(typeof(MV_Project))]
    public class MV_ProjectInspector : Editor
    {
        public VisualTreeAsset _inspectorTree;
        public VisualTreeAsset _levelInspectorTree;

        private List<MV_Level> _levels = new();
        private List<MV_Level> _searchableLevels = new();

        private ListView _listLevels;
        private TextField _fieldFilterName;
        private Button _buttonFilter;

        public override VisualElement CreateInspectorGUI()
        {
            _levels = MV_Project.Instance.GetLevels();

            // Create a new VisualElement to be the root of our Inspector UI.
            VisualElement myInspector = new();
            TemplateContainer template = _inspectorTree.Instantiate();

            // Fetching 
            _fieldFilterName = template.Q<TextField>("field-filter-name");

            _buttonFilter = template.Q<Button>("button-filter");
            _buttonFilter.clicked += OnFilterButtonClicked;

            _listLevels = template.Q<ListView>("list-levels");
            _listLevels.makeItem = () => new MV_LevelsListElement(_levelInspectorTree);
            _listLevels.bindItem = (e, i) =>
            {
                MV_LevelsListElement item = e as MV_LevelsListElement;
                item.Level = _searchableLevels[i];
            };

            PopulateSearchablesWithAll();

            myInspector.Add(template);
            // Return the finished Inspector UI.
            return myInspector;
        }

        private void OnFilterButtonClicked()
        {
            string term = _fieldFilterName.text.ToLower();

            if (string.IsNullOrEmpty(term))
            {
                PopulateSearchablesWithAll();
                return;
            }

            _searchableLevels = _levels.FindAll(x => x.Name.ToLower().Contains(term));
            _listLevels.itemsSource = _searchableLevels;

            _listLevels.RefreshItems();
        }

        private void PopulateSearchablesWithAll()
        {
            _searchableLevels.Clear();
            _searchableLevels.Capacity = _levels.Count; // pre-allocate capacity to avoid resizing
            foreach (MV_Level level in _levels)
            {
                _searchableLevels.Add(level);
            }

            _listLevels.itemsSource = _searchableLevels;
            _listLevels.RefreshItems();
        }
    }
}
