using LDtkUnity;
using LDtkLevelManager;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkLevelManagerEditor
{
    public class ProjectLanesViewElement : VisualElement
    {
        #region Fields

        private const string TemplateName = "ProjectInspector_LanesView";

        private Project _project;
        private LdtkJson _ldtkJson;

        private TemplateContainer _containerMain;
        private ObjectField _fieldMapEditorScene;

        private Slider _fieldCameraNegativeOffset;
        private LaneInfoField _fieldUniverseLane;
        private LaneInfoField _fieldMapRenderingLane;

        #endregion

        #region Constructors

        public ProjectLanesViewElement(Project project)
        {
            _project = project;
            _ldtkJson = _project.LDtkProject;

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();

            _fieldCameraNegativeOffset = _containerMain.Q<Slider>("field-camera-negative-offset");
            _fieldCameraNegativeOffset.SetValueWithoutNotify(_project.LanesSettings.CameraNegativeOffset);
            _fieldCameraNegativeOffset.RegisterValueChangedCallback(evt =>
            {
                _project.LanesSettings.CameraNegativeOffset = _fieldCameraNegativeOffset.value;
            });

            _fieldUniverseLane = _containerMain.Q<LaneInfoField>("field-universe-lane");
            _fieldUniverseLane.Initialize(_project.LanesSettings.UniverseLane);

            _fieldMapRenderingLane = _containerMain.Q<LaneInfoField>("field-map-rendering-lane");
            _fieldMapRenderingLane.Initialize(_project.LanesSettings.MapRenderingLane);

            EditorUtility.SetDirty(_project);
            Add(_containerMain);
        }

        #endregion

        #region Callbacks

        #endregion
    }
}