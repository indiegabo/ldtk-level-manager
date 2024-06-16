
using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using LDtkVania;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class ProjectSettingsViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_SettingsView";

        private MV_Project _project;
        private LdtkJson _ldtkJson;

        private TemplateContainer _containerMain;

        #endregion

        #region Constructors

        public ProjectSettingsViewElement(MV_Project project)
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