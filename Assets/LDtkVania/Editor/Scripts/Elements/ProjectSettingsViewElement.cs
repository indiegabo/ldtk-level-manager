using LDtkUnity;
using LDtkVania;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class ProjectSettingsViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_SettingsView";

        private Project _project;
        private LdtkJson _ldtkJson;

        private TemplateContainer _containerMain;
        private ObjectField _fieldMapEditorScene;

        #endregion

        #region Constructors

        public ProjectSettingsViewElement(Project project)
        {
            _project = project;
            _ldtkJson = _project.LDtkProject;

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();


            Add(_containerMain);
        }

        #endregion

        #region Callbacks

        #endregion
    }
}