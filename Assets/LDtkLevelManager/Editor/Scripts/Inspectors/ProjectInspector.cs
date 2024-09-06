using System.Collections.Generic;
using LDtkUnity;
using LDtkLevelManager;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkLevelManagerEditor
{
    [CustomEditor(typeof(Project))]
    public class ProjectInspector : Editor
    {
        #region Fields

        private const string TemplateName = "ProjectInspector";

        private TemplateContainer _containerMain;
        private TabViewElement _tabViewElement;
        private ObjectField _fieldLDtkProject;
        private Button _buttonResync;

        private ProgressBar _progressBar;

        #endregion

        private Project _project;

        public override VisualElement CreateInspectorGUI()
        {
            _project = target as Project;

            _progressBar = new ProgressBar
            {
                title = "Loading project..."
            };

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();
            _fieldLDtkProject = _containerMain.Q<ObjectField>("field-ldtk-project");
            _fieldLDtkProject.objectType = typeof(LDtkProjectFile);
            _fieldLDtkProject.SetValueWithoutNotify(_project.LDtkProjectFile);
            _fieldLDtkProject.SetEnabled(!_project.IsInitialized);

            _fieldLDtkProject.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                LDtkProjectFile projectFile = e.newValue as LDtkProjectFile;
                if (projectFile == null) return;
                _project.Initialize(projectFile);
                EvaluateTabViewPresence(_project.IsInitialized);
            });

            _buttonResync = _containerMain.Q<Button>("button-resync");
            _buttonResync.clicked += () =>
            {
                _project.ReSync();
            };

            _tabViewElement = new();
            _tabViewElement.AddToClassList("container");
            _containerMain.Add(_tabViewElement);
            ProjectMainViewElement mainViewElement = new(_project);
            ProjectLevelsViewElement levelsViewElement = new(_project);

            _tabViewElement.AddTab("Main", mainViewElement);
            _tabViewElement.AddTab("Levels", levelsViewElement);

            if (string.IsNullOrEmpty(TabViewElement.LastUsedTab))
            {
                _tabViewElement.SelectTab("Main");
            }

            EvaluateTabViewPresence(_project.IsInitialized);

            return _containerMain;
        }

        private void EvaluateTabViewPresence(bool hasProjectFile)
        {
            _tabViewElement.style.display = hasProjectFile ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
