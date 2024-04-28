using UnityEngine;
using UnityEngine.UIElements;
using LDtkVania;
using UnityEditor.UIElements;
using LDtkUnity;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace LDtkVaniaEditor
{
    public class ProjectLevelsViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_LevelsView";

        private MV_Project _project;
        private List<MV_Level> _levels;
        private List<MV_Level> _searchableLevels = new();

        private TemplateContainer _containerMain;
        private ListView _listLevels;
        private TextField _fieldFilterName;
        private Button _buttonFilter;
        private Button _buttonSyncLevels;

        #endregion

        #region Properties

        #endregion

        #region Constructors

        public ProjectLevelsViewElement(MV_Project project)
        {
            _project = project;
            _levels = _project.GetLevels();

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldFilterName = _containerMain.Q<TextField>("field-filter-name");
            _fieldFilterName.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    ApplyFilters();
                }
            }, TrickleDown.TrickleDown);

            _buttonFilter = _containerMain.Q<Button>("button-filter");
            _buttonFilter.clicked += ApplyFilters;

            _listLevels = _containerMain.Q<ListView>("list-levels");
            _listLevels.makeItem = () => new LevelListItemElement();
            _listLevels.bindItem = (e, i) =>
            {
                LevelListItemElement item = e as LevelListItemElement;
                item.Level = _searchableLevels[i];
            };

            _buttonSyncLevels = _containerMain.Q<Button>("button-sync-levels");
            _buttonSyncLevels.clicked += () => _project.SyncLevels();

            PopulateSearchablesWithAll();
            Add(_containerMain);
        }

        #endregion

        private void ApplyFilters()
        {
            string nameTerm = _fieldFilterName.text.ToLower();

            if (string.IsNullOrEmpty(nameTerm))
            {
                PopulateSearchablesWithAll();
                return;
            }

            _searchableLevels = _levels.FindAll(level => level.name.ToLower().Contains(nameTerm));

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