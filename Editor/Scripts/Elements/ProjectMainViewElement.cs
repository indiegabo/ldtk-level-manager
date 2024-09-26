
using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using LDtkLevelManager;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace LDtkLevelManagerEditor
{
    public class ProjectMainViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_MainView";

        private Project _project;
        private LdtkJson _ldtkJson;

        private TemplateContainer _containerMain;

        private VisualElement _containerWorlds;
        private EnumField _organizationField;
        private EnumField _strategyField;
        private SliderInt _neighbouringDepthField;
        private DropdownField _dropdownNavigationLayer;
        private Slider _sliderScaleFactor;
        private ListView _listWorlds;

        private List<WorldInfo> _worldAreas;
        private List<string> _layers;

        #endregion

        #region Constructors

        public ProjectMainViewElement(Project project)
        {
            _project = project;
            _ldtkJson = _project.LDtkProject;
            _worldAreas = _project.GetAllWorldInfos();
            _layers = _ldtkJson.Defs.Layers.Select(x => x.Identifier).ToList();

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _organizationField = _containerMain.Q<EnumField>("field-organization");
            _organizationField.Init(_project.Organization);
            _organizationField.RegisterValueChangedCallback(OnOrganizationChanged);

            _strategyField = _containerMain.Q<EnumField>("field-strategy");
            _strategyField.Init(_project.Strategy);
            _strategyField.RegisterValueChangedCallback(OnStrategyChanged);

            _neighbouringDepthField = _containerMain.Q<SliderInt>("field-neighbouring-depth");
            _neighbouringDepthField.SetValueWithoutNotify(_project.NeighbouringDepth);
            _neighbouringDepthField.RegisterValueChangedCallback(x =>
            {
                _project.SetNeighbouringDepth(x.newValue);
            });

            _dropdownNavigationLayer = _containerMain.Q<DropdownField>("dropdown-navigation-layer");
            _dropdownNavigationLayer.choices = _layers;
            _dropdownNavigationLayer.value = _project.NavigationLayer;
            _dropdownNavigationLayer.RegisterValueChangedCallback(x =>
            {
                _project.SetNavigationLayer(x.newValue);
                EditorUtility.SetDirty(_project);
            });

            _sliderScaleFactor = _containerMain.Q<Slider>("slider-scale-factor");
            _sliderScaleFactor.SetValueWithoutNotify(_project.Cartography.scaleFactor);
            _sliderScaleFactor.RegisterValueChangedCallback(x =>
            {
                _project.Cartography.scaleFactor = x.newValue;
                EditorUtility.SetDirty(_project);
            });

            _listWorlds = _containerMain.Q<ListView>("list-worlds");
            _listWorlds.itemsSource = _worldAreas;
            _listWorlds.makeItem = CreateWorldFoldout;
            _listWorlds.bindItem = BindWorldFoldout;

            EvaluteHiddenFields();

            Add(_containerMain);
        }

        #endregion

        #region Worlds

        private VisualElement CreateWorldFoldout()
        {
            return new VisualElement();
        }

        private void BindWorldFoldout(VisualElement element, int index)
        {
            element.Clear();
            WorldInfo worldAreas = _worldAreas[index];
            if (worldAreas.areas.Count == 0)
            {
                element.Add(new Label(worldAreas.name));
            }
            else
            {
                Foldout foldout = new()
                {
                    text = _worldAreas[index].name,
                    value = false
                };

                VisualElement labelsContainer = new();
                foreach (string area in _worldAreas[index].areas)
                {
                    Label label = new()
                    {
                        text = area
                    };
                    labelsContainer.Add(label);
                }
                foldout.Add(labelsContainer);
                element.Add(foldout);
            }
        }

        #endregion

        #region View

        private void EvaluteHiddenFields()
        {
            Project.LevelsOrganization organization = (Project.LevelsOrganization)_organizationField.value;
            Project.ConnectedLoadingStrategy strategy = (Project.ConnectedLoadingStrategy)_strategyField.value;

            if (organization == Project.LevelsOrganization.Connected)
            {
                _strategyField.style.display = DisplayStyle.Flex;
                if (strategy == Project.ConnectedLoadingStrategy.Neighbours)
                {
                    _neighbouringDepthField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _neighbouringDepthField.style.display = DisplayStyle.None;
                }
                _dropdownNavigationLayer.style.display = DisplayStyle.Flex;
            }
            else if (organization == Project.LevelsOrganization.Unrelated)
            {
                _strategyField.style.display = DisplayStyle.None;
                _neighbouringDepthField.style.display = DisplayStyle.None;
                _dropdownNavigationLayer.style.display = DisplayStyle.None;
            }
        }

        #endregion

        #region Callbacks

        private void OnOrganizationChanged(ChangeEvent<System.Enum> @event)
        {
            Project.LevelsOrganization organization = (Project.LevelsOrganization)@event.newValue;
            _project.SetOrtanization(organization);
            EvaluteHiddenFields();
        }

        private void OnStrategyChanged(ChangeEvent<System.Enum> @event)
        {
            Project.ConnectedLoadingStrategy strategy = (Project.ConnectedLoadingStrategy)@event.newValue;
            _project.SetConnectedLoadingStrategy(strategy);
            EvaluteHiddenFields();
        }

        #endregion
    }
}