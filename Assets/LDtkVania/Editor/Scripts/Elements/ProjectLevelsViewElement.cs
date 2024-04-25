using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using LDtkUnity;
using UnityEditor;
using System.Collections.Generic;

namespace LDtkVaniaEditor
{
    public class ProjectLevelsViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_LevelsView";

        private List<MV_Level> _levels;
        private List<MV_Level> _searchableLevels = new();

        private TemplateContainer _containerMain;
        private ListView _listLevels;
        private TextField _fieldFilterName;
        private Button _buttonFilter;

        #endregion

        #region Properties

        #endregion

        #region Constructors

        public ProjectLevelsViewElement(List<MV_Level> levels)
        {
            _levels = levels;

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldFilterName = _containerMain.Q<TextField>("field-filter-name");

            _buttonFilter = _containerMain.Q<Button>("button-filter");
            _buttonFilter.clicked += OnFilterButtonClicked;

            _listLevels = _containerMain.Q<ListView>("list-levels");
            _listLevels.makeItem = () => new LevelListItemElement();
            _listLevels.bindItem = (e, i) =>
            {
                LevelListItemElement item = e as LevelListItemElement;
                item.Level = _searchableLevels[i];
            };

            PopulateSearchablesWithAll();
            Add(_containerMain);
        }

        #endregion

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