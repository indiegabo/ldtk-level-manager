
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class ProjectMainViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_Main";

        private TemplateContainer _containerMain;

        #endregion

        #region Constructors

        public ProjectMainViewElement()
        {
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();
            Add(_containerMain);
        }

        #endregion
    }
}