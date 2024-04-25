
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
        private Button _buttonSyncLevels;

        #endregion

        #region Constructors

        public ProjectMainViewElement(MV_Project project)
        {
            _project = project;
            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();
            _buttonSyncLevels = _containerMain.Q<Button>("button-sync-levels");
            _buttonSyncLevels.clicked += OnSyncLevelsRequested;
            Add(_containerMain);
        }

        #endregion

        #region Callbacks

        private void OnSyncLevelsRequested()
        {
            _project.SyncLevels();
        }

        #endregion
    }
}