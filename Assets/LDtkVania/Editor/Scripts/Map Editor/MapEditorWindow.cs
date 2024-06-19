using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using LDtkVania;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using LDtkUnity;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace LDtkVaniaEditor
{
    public delegate void ProjectSelected(MV_Project project);
    public class MapEditorWindow : EditorWindow
    {
        #region Static

        private static readonly string TemplateName = "MapEditorWindow";

        private static MapEditorWindow _window;

        [MenuItem("Window/LdtkVania/Map Editor")]
        public static void ShowWindow()
        {
            if (_window == null)
            {
                _window = GetWindow<MapEditorWindow>();
                _window.titleContent = new GUIContent("LDtkVania - Map Editor");
            }
            else
            {
                _window.Show();
            }
        }

        #endregion

        #region Fields

        private Dictionary<string, MV_Project> _projects;
        private Dictionary<string, World> _selectedProjectWorlds;

        private TemplateContainer _containerMain;

        private ObjectField _fieldMapEditorScene;
        private DropdownField _dropdownProject;
        private DropdownField _dropdownWorld;
        private Button _buttonOpenScene;
        private Button _buttonClear;
        private MapView _mapView;

        private event ProjectSelected _projectSelected;

        #endregion

        #region Properties

        protected MapEditorSettings Settings => MapEditorSettings.instance;

        #endregion

        #region Life Cycle

        public void CreateGUI()
        {
            _projects = new();
            _selectedProjectWorlds = new();

            List<MV_Project> projects = MV_Project.FindAllProjects();

            foreach (MV_Project project in projects)
            {
                _projects.Add(project.name, project);
            }

            VisualElement root = rootVisualElement;

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").CloneTree();
            _containerMain.style.flexGrow = 1;

            _dropdownProject = _containerMain.Q<DropdownField>("dropdown-project");
            InitializeProjectsDropdown();

            _dropdownWorld = _containerMain.Q<DropdownField>("dropdown-world");

            _fieldMapEditorScene = _containerMain.Q<ObjectField>("field-map-editor-scene");
            _fieldMapEditorScene.SetValueWithoutNotify(Settings.MapScene);
            _fieldMapEditorScene.RegisterValueChangedCallback(x => Settings.MapScene = x.newValue as SceneAsset);

            _buttonOpenScene = _containerMain.Q<Button>("button-open-scene");
            _buttonOpenScene.clicked += OpenMapEditorScene;

            _buttonClear = _containerMain.Q<Button>("button-clear");
            _buttonClear.clicked += ClearLoadedLevels;

            _mapView = _containerMain.Q<MapView>("map-view");
            _mapView.SetSelectionAnalysisCallback(OnLevelSelectionChanged);
            _mapView.SetLevelLoadRequestCallback(OnLevelLoadRequest);

            if (Settings.HasMapScene && Settings.HasCurrentProject && _projects.ContainsKey(Settings.CurrentProject.name))
            {
                RebuildCurrentState();
            }
            else
            {
                Settings.CurrentProject = null;
                Settings.ResetState();
                EvaluateProjectSelection();
            }

            root.Add(_containerMain);
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            _projectSelected += OnProjectSelected;
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            _projectSelected -= OnProjectSelected;
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!Settings.HasMapScene)
            {
                ClearLoadedLevels();
                Settings.ResetState();
                return;
            }

            bool isMapSceneOpen = IsMapSceneOpen();

            if (!isMapSceneOpen)
            {
                ClearLoadedLevels();
                Settings.ReleaseLevels();
                return;
            }
        }

        #endregion

        #region Projects

        private void InitializeProjectsDropdown()
        {
            _dropdownProject.choices = _projects.Values.Select(x => x.name).ToList();
            _dropdownProject.RegisterValueChangedCallback(x => SelectProject(x.newValue));
        }

        private void RebuildCurrentState()
        {
            _dropdownProject.SetValueWithoutNotify(Settings.CurrentProject.name);
            InitializeWorldsDropdown(Settings.CurrentProject);

            if (Settings.HasInitializedWorldName && CurrentProjectContainsWorld(Settings.InitializedWorldName))
            {
                _dropdownWorld.SetValueWithoutNotify(Settings.InitializedWorldName);
                LoadWorldSilently(Settings.InitializedWorldName);
                return;
            }

            if (_selectedProjectWorlds.Count > 0)
            {
                string worldName = _selectedProjectWorlds.First().Value.Identifier;
                _dropdownWorld.SetValueWithoutNotify(worldName);
                SelectWorld(worldName);
                return;
            }

            void LoadWorldSilently(string worldName)
            {
                ClearLoadedLevels();
                Settings.InitializedWorldName = worldName;
                World world = _selectedProjectWorlds[worldName];
                _mapView.InitializeWorld(Settings.CurrentProject, world, Settings.MapViewTransform);
            }
        }

        private void EvaluateProjectSelection()
        {
            if (_projects.Count > 0)
            {
                string projectName = _projects.First().Value.name;
                _dropdownProject.SetValueWithoutNotify(projectName);
                SelectProject(_dropdownProject.value);
                return;
            }
        }

        private void SelectProject(string projectName)
        {
            SelectProject(_projects[projectName]);
        }

        private void SelectProject(MV_Project project)
        {
            Settings.CurrentProject = project;
            _projectSelected?.Invoke(project);
        }

        private void OnProjectSelected(MV_Project project)
        {
            InitializeWorldsDropdown(Settings.CurrentProject);

            if (_selectedProjectWorlds.Count > 0)
            {
                string worldName = _selectedProjectWorlds.Values.First().Identifier;
                _dropdownWorld.SetValueWithoutNotify(worldName);
                SelectWorld(worldName);
            }
        }

        private bool CurrentProjectContainsWorld(string worldName)
        {
            return _selectedProjectWorlds.ContainsKey(worldName);
        }

        #endregion

        #region Worlds

        private void InitializeWorldsDropdown(MV_Project project)
        {
            List<World> worlds = project.LDtkProject.Worlds.ToList();
            _selectedProjectWorlds.Clear();

            foreach (World world in worlds)
            {
                _selectedProjectWorlds.Add(world.Identifier, world);
            }

            _dropdownWorld.choices = worlds.Select(x => x.Identifier).ToList();
            _dropdownWorld.RegisterValueChangedCallback(x => SelectWorld(x.newValue));
        }

        private void SelectWorld(string worldName)
        {
            ClearLoadedLevels();
            Settings.InitializedWorldName = worldName;
            World world = _selectedProjectWorlds[worldName];
            rootVisualElement.schedule.Execute(() => _mapView.InitializeWorld(Settings.CurrentProject, world));
        }

        #endregion

        #region Loading Levels

        private void OnLevelLoadRequest(MapLevelElement element)
        {
            if (!IsMapSceneOpen()) { return; }

            if (!Settings.IsLevelLoaded(element.MVLevel))
            {
                LoadedLevelEntry entry = LoadLevel(element.MVLevel);
                element.RegisterLoadedEntry(entry);
            }
            else
            {
                UnloadLevel(element.MVLevel);
            }
        }

        private LoadedLevelEntry LoadLevel(MV_Level mvLevel)
        {
            LoadedLevelEntry entry;
            if (mvLevel.HasScene)
            {
                string path = AssetDatabase.GUIDToAssetPath(mvLevel.Scene.AssetGuid);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                entry = Settings.RegisterLoadedLevel(mvLevel, scene);
            }
            else
            {
                GameObject obj = Instantiate(mvLevel.Asset) as GameObject;
                obj.name = mvLevel.Name;
                entry = Settings.RegisterLoadedLevel(mvLevel, obj);
            }
            return entry;
        }

        private void UnloadLevel(MV_Level mvLevel)
        {
            if (!Settings.TryGetLoadedLevel(mvLevel.Iid, out LoadedLevelEntry loadedLevelEntry)) return;

            Settings.UnregisterLoadedLevel(mvLevel.Iid);

            if (mvLevel.HasScene)
            {
                string path = AssetDatabase.GUIDToAssetPath(mvLevel.Scene.AssetGuid);
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                Scene scene = EditorSceneManager.GetSceneByName(sceneAsset.name);
                if (scene != null && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            else
            {
                DestroyImmediate(loadedLevelEntry.LoadedObject);
            }
        }

        private void ClearLoadedLevels()
        {
            List<LoadedLevelEntry> entries = Settings.GetLoadedLevels();
            foreach (LoadedLevelEntry entry in entries)
            {
                UnloadLevel(entry.MVLevel);
            }
        }

        private void OnLevelSelectionChanged(List<ISelectable> selectables)
        {
            if (selectables.Count == 0)
            {
                Debug.Log("No level selected.");
                return;
            }

            if (selectables.Count == 1)
            {
                MapLevelElement mapLevelElement = selectables[0] as MapLevelElement;
                Debug.Log($"Selected {mapLevelElement.Level.Identifier}");
                return;
            }

            Debug.Log($"Selected {selectables.Count} levels.");
        }

        #endregion

        #region Editor Scene

        private void OpenMapEditorScene()
        {
            if (!Settings.HasMapScene) return;

            if (IsMapSceneOpen())
            {
                return;
            }

            try
            {
                string path = AssetDatabase.GetAssetPath(Settings.MapScene);
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(path);
                }
            }
            catch (System.Exception e)
            {
                MV_Logger.Exception(e);
            }
        }

        private bool IsMapSceneOpen()
        {
            if (!Settings.HasMapScene)
            {
                Settings.ResetState();
                return false;
            }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            bool isOpen = activeScene.name == Settings.MapScene.name;

            if (!isOpen)
            {
                ClearLoadedLevels();
                Settings.ReleaseLevels();
            }

            return isOpen;
        }

        #endregion
    }
}