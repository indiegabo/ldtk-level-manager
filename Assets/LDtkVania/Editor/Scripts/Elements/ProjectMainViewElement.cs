
using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using LDtkVania;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class ProjectMainViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_MainView";

        private MV_Project _project;
        private LdtkJson _ldtkJson;

        private TemplateContainer _containerMain;

        private VisualElement _containerWorlds;
        private DropdownField _dropdownNavigationLayer;
        private ListView _listWorlds;

        private List<MV_WorldAreas> _worldAreas;
        private List<string> _layers;

        #endregion

        #region Constructors

        public ProjectMainViewElement(MV_Project project)
        {
            _project = project;
            _ldtkJson = _project.LDtkProject;
            _worldAreas = _project.GetAllWorldAreas();
            _layers = _ldtkJson.Defs.Layers.Select(x => x.Identifier).ToList();

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _dropdownNavigationLayer = _containerMain.Q<DropdownField>("dropdown-navigation-layer");
            _dropdownNavigationLayer.choices = _layers;
            _dropdownNavigationLayer.value = _project.NavigationLayer;
            _dropdownNavigationLayer.RegisterValueChangedCallback(x => _project.SetNavigationLayer(x.newValue));

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
            MV_WorldAreas worldAreas = _worldAreas[index];
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