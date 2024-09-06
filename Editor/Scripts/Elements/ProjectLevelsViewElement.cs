using UnityEngine;
using UnityEngine.UIElements;
using LDtkLevelManager;
using UnityEditor.UIElements;
using LDtkUnity;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace LDtkLevelManagerEditor
{
    public class ProjectLevelsViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_LevelsView";

        private Project _project;
        private List<LDtkLevelManager.LevelInfo> _leftBehind;
        private List<LDtkLevelManager.LevelInfo> _searchableLevels = new();

        private TemplateContainer _containerMain;
        private ListView _listLevels;
        private ListView _listLeftBehind;
        private DropdownField _fieldFilterWorld;
        private DropdownField _fieldFilterArea;
        private TextField _fieldFilterName;
        private Button _buttonFilter;
        private Button _buttonSyncLevels;
        private Label _labelTotalOfLevels;
        private PaginatorElement _paginatorElement;

        #endregion

        #region Properties

        #endregion

        #region Constructors

        public ProjectLevelsViewElement(Project project)
        {
            _project = project;
            _leftBehind = _project.GetAllLeftBehind();

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldFilterWorld = _containerMain.Q<DropdownField>("field-filter-world");
            List<string> worldChoices = new()
            {
                "None",
            };

            foreach (WorldInfo worldAreas in _project.GetAllWorldInfos())
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

            _labelTotalOfLevels = _containerMain.Q<Label>("label-total-of-levels-number");
            _labelTotalOfLevels.text = _project.LevelsCount.ToString();

            VisualElement containerLabelPagination = _containerMain.Q<VisualElement>("container-label-pagination");

            _paginatorElement = new();
            _paginatorElement.style.flexGrow = 1;
            _paginatorElement.style.flexShrink = 0;
            _paginatorElement.PaginationChanged += pagination => Paginate();
            _paginatorElement.TotalOfItems = _project.LevelsCount;
            containerLabelPagination.Add(_paginatorElement);

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
            _buttonSyncLevels.clicked += () => _project.ReSync();

            EvaluateAreaFilter("None");
            Paginate();

            // World world = projectJSON.Worlds.FirstOrDefault(w => w.Levels.Any(l => l.Iid == _iid));
            // _world = world?.Identifier;
            Add(_containerMain);
        }

        #endregion

        private void Paginate()
        {
            LevelListFilters filters = new()
            {
                world = _fieldFilterWorld.value == "None" ? null : _fieldFilterWorld.value,
                area = _fieldFilterArea.value == "None" ? null : _fieldFilterArea.value,
                levelName = _fieldFilterName.value.ToLower(),
            };

            PaginationInfo pagination = _paginatorElement.Pagination;

            PaginatedResponse<LDtkLevelManager.LevelInfo> response = _project.GetPaginatedLevels(filters, pagination);
            _paginatorElement.TotalOfItems = response.TotalCount;
            _searchableLevels = response.Items;
            _listLevels.itemsSource = _searchableLevels;

            _listLevels.RefreshItems();
        }

        private void EvaluateAreaFilter(string selectedWorld)
        {
            if (selectedWorld == "None")
            {
                ClearAreaFilter();
                return;
            }
            _project.WorldAreas.TryGetValue(selectedWorld, out WorldInfo worldAreas);

            if (worldAreas.areas.Count == 0)
            {
                ClearAreaFilter();
                return;
            }

            _fieldFilterArea.style.display = DisplayStyle.Flex;
            List<string> areaChoices = new()
            {
                "None"
            };
            areaChoices.AddRange(worldAreas.areas);
            _fieldFilterArea.choices = areaChoices;

            void ClearAreaFilter()
            {
                _fieldFilterArea.choices = new()
                {
                    "None"
                };
                _fieldFilterArea.SetValueWithoutNotify("");
                _fieldFilterArea.style.display = DisplayStyle.None;
            }
        }
    }
}