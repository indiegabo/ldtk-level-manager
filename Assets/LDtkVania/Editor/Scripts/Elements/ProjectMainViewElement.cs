
using System.Collections.Generic;
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

        private TemplateContainer _containerMain;

        private VisualElement _containerWorlds;
        private ListView _listWorlds;

        private List<MV_WorldAreas> _worldAreas;

        #endregion

        #region Constructors

        public ProjectMainViewElement(MV_Project project)
        {
            _project = project;
            _worldAreas = _project.GetAllWorldAreas();

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

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