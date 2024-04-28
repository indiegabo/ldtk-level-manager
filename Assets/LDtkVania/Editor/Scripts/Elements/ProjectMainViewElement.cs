
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
        private Foldout _foldoutWorlds;
        private DropdownField _fieldWorlds;

        #endregion

        #region Constructors

        public ProjectMainViewElement(MV_Project project)
        {
            _project = project;
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _foldoutWorlds = _containerMain.Q<Foldout>("foldout-worlds");
            _foldoutWorlds.value = false;
            FillWorldsFoldout(_foldoutWorlds, _project);

            _fieldWorlds = _containerMain.Q<DropdownField>("field-worlds");
            FillWorldsDropdown(_fieldWorlds, _project);

            Add(_containerMain);
        }

        #endregion

        #region Worlds

        private void FillWorldsFoldout(Foldout foldout, MV_Project project)
        {
            World[] worlds = project.LDtkProject.Worlds;
            for (int i = 0; i < worlds.Length; i++)
            {
                World world = worlds[i];
                Label labelWorldName = new() { text = world.Identifier };
                labelWorldName.AddToClassList("world-name");
                foldout.Add(labelWorldName);
            }
        }

        private void FillWorldsDropdown(DropdownField dropdown, MV_Project project)
        {
            World[] worlds = project.LDtkProject.Worlds;
            for (int i = 0; i < worlds.Length; i++)
            {
                World world = worlds[i];
                dropdown.choices.Add(world.Identifier);
            }
        }

        #endregion

        #region Callbacks

        #endregion
    }
}