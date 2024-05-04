
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

        #endregion

        #region Constructors

        public ProjectMainViewElement(MV_Project project)
        {
            _project = project;

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            Add(_containerMain);
        }

        #endregion

        #region Worlds

        #endregion

        #region Callbacks

        #endregion
    }
}