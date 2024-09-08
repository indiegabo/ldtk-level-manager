
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
            // (element, i) => (element as Label).text = _worldAreas[i].worldName
            element.Clear();
            WorldInfo worldAreas = _worldAreas[index];
            if (worldAreas.areas.Count == 0)
            {
                element.Add(new Label(worldAreas.worldName));
            }
            else
            {
                Foldout foldout = new()
                {
                    text = _worldAreas[index].worldName
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

        #region Callbacks

        #endregion
    }
}