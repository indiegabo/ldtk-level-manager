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
        private DropdownField _fieldFilterWorld;
        private DropdownField _fieldFilterArea;
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
            _leftBehind = _project.GetAllLeftBehind();

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldFilterWorld = _containerMain.Q<DropdownField>("field-filter-world");
            List<string> worldChoices = new()
            {
                "None",
            };

            foreach (MV_WorldAreas worldAreas in _project.GetAllWorldAreas())
            {
                worldChoices.Add(worldAreas.worldName);
            }

            _fieldFilterWorld.choices = worldChoices;
            _fieldFilterWorld.SetValueWithoutNotify(worldChoices[0]);
            _fieldFilterWorld.RegisterValueChangedCallback(evt => EvaluateAreaFilter(evt.newValue));

            _fieldFilterArea = _containerMain.Q<DropdownField>("field-filter-area");
            _fieldFilterName = _containerMain.Q<TextField>("field-filter-name");

            _buttonFilter = _containerMain.Q<Button>("button-filter");
            _buttonFilter.clicked += Paginate;

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

            EvaluateAreaFilter("None");
            Paginate();

            // World world = projectJSON.Worlds.FirstOrDefault(w => w.Levels.Any(l => l.Iid == _iid));
            // _world = world?.Identifier;
            Add(_containerMain);
        }

        #endregion

        private void Paginate()
        {
            MV_LevelListFilters filters = new()
            {
                world = _fieldFilterWorld.value == "None" ? null : _fieldFilterWorld.value,
                area = _fieldFilterArea.value == "None" ? null : _fieldFilterArea.value,
                levelName = _fieldFilterName.value.ToLower(),
            };

            MV_PaginationInfo pagination = new()
            {
                PageIndex = 1,
                PageSize = 10,
            };

            _searchableLevels = _project.GetPaginatedLevels(filters, pagination).Items;
            _listLevels.itemsSource = _searchableLevels;

            _listLevels.RefreshItems();
        }

        private void EvaluateAreaFilter(string newValue)
        {
            if (newValue == "None")
            {
                ClearAreaFilter();
                return;
            }
            _project.WorldAreas.TryGetValue(newValue, out MV_WorldAreas worldAreas);

            if (worldAreas.areas.Count == 0)
            {
                ClearAreaFilter();
                return;
            }

            _fieldFilterArea.style.display = DisplayStyle.Flex;
            _fieldFilterArea.choices = worldAreas.areas;

            void ClearAreaFilter()
            {
                _fieldFilterArea.choices = new()
                {
                    ""
                };
                _fieldFilterArea.SetValueWithoutNotify("");
                _fieldFilterArea.style.display = DisplayStyle.None;
            }
        }
    }
}