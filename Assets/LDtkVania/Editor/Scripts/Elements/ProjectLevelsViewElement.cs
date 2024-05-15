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
        private List<MV_Level> _leftBehind;
        private List<MV_Level> _searchableLevels = new();

        private TemplateContainer _containerMain;
        private ListView _listLevels;
        private ListView _listLeftBehind;
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

            MV_LevelListFilters filters = new()
            {
                world = "city",
                area = "factory",
            };

            MV_PaginationInfo pagination = new()
            {
                PageIndex = 2,
                PageSize = 2,
            };

            MV_PaginatedResponse<MV_Level> response = _project.GetPaginatedLevels(filters, pagination);
            Debug.Log($"Levels: {response.TotalCount}");
            _levels = response.Items;
            _leftBehind = _project.GetAllLeftBehind();

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

            _listLeftBehind = _containerMain.Q<ListView>("list-left-behind");
            _listLeftBehind.makeItem = () => new LevelListItemElement();
            _listLeftBehind.bindItem = (e, i) =>
            {
                LevelListItemElement item = e as LevelListItemElement;
                item.Level = _leftBehind[i];
            };

            _listLeftBehind.itemsSource = _leftBehind;

            _buttonSyncLevels = _containerMain.Q<Button>("button-sync-levels");
            _buttonSyncLevels.clicked += () => _project.SyncLevels();

            PopulateSearchablesWithAll();

            // World world = projectJSON.Worlds.FirstOrDefault(w => w.Levels.Any(l => l.Iid == _iid));
            // _world = world?.Identifier;
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