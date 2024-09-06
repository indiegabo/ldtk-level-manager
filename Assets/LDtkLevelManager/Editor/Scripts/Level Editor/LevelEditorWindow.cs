using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using LDtkLevelManager;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using LDtkUnity;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace LDtkLevelManagerEditor
{
    public delegate void ProjectSelected(Project project);
    public class LevelEditorWindow : EditorWindow
    {
        #region Static

        private static readonly string TemplateName = "LevelEditorWindow";

        private static LevelEditorWindow _window;

        [MenuItem("Window/LDtkLevelManager/Level Editor")]
        public static void ShowWindow()
        {
            if (_window == null)
            {
                _window = GetWindow<LevelEditorWindow>();
                _window.titleContent = new GUIContent("Level Editor");
            }
            else
            {
                _window.Show();
            }
        }

        #endregion

        #region Fields

        private Dictionary<string, Project> _projects;
        private Dictionary<string, World> _selectedProjectWorlds;

        private TemplateContainer _containerMain;

        private ObjectField _fieldLevelEditorScene;
        private ObjectField _fieldUniverseScene;
        private DropdownField _dropdownProject;
        private DropdownField _dropdownWorld;
        private VisualElement _containerSelectedLevel;
        private Label _labelSelectedLevel;
        private ScrollView _scrollViewSelectedLevel;
        private Button _buttonOpenLevelEditorScene;
        private Button _buttonOpenUniverseScene;
        private Button _buttonUnloadAll;
        private Button _buttonLoadWorld;
        private Button _buttonLoadSelection;
        private MapView _mapView;
        private ScrollView _scrollViewSelectedLevels;

        private List<ISelectable> _currentlySelectedLevels;

        private event ProjectSelected _projectSelected;

        #endregion

        #region Properties

        protected LevelEditorSettings Settings => LevelEditorSettings.instance;

        #endregion

        #region Life Cycle

        public void CreateGUI()
        {
            _projects = new();
            _selectedProjectWorlds = new();
            _currentlySelectedLevels = new();

            List<Project> projects = Project.FindAllProjects();

            foreach (Project project in projects)
            {
                _projects.Add(project.name, project);
            }

            VisualElement root = rootVisualElement;

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").CloneTree();
            _containerMain.style.flexGrow = 1;

            _dropdownProject = _containerMain.Q<DropdownField>("dropdown-project");
            InitializeProjectsDropdown();

            _dropdownWorld = _containerMain.Q<DropdownField>("dropdown-world");

            _fieldLevelEditorScene = _containerMain.Q<ObjectField>("field-level-editor-scene");
            _fieldLevelEditorScene.SetValueWithoutNotify(Settings.LevelEditorScene);
            _fieldLevelEditorScene.RegisterValueChangedCallback(x => Settings.LevelEditorScene = x.newValue as SceneAsset);

            _buttonOpenLevelEditorScene = _containerMain.Q<Button>("button-open-level-editor-scene");
            _buttonOpenLevelEditorScene.clicked += OpenMapEditorScene;

            _fieldUniverseScene = _containerMain.Q<ObjectField>("field-universe-scene");
            _fieldUniverseScene.SetValueWithoutNotify(Settings.UniverseScene);
            _fieldUniverseScene.RegisterValueChangedCallback(x => Settings.UniverseScene = x.newValue as SceneAsset);

            _buttonOpenUniverseScene = _containerMain.Q<Button>("button-open-universe-scene");
            _buttonOpenUniverseScene.clicked += OpenUniverseScene;

            _containerSelectedLevel = _containerMain.Q<VisualElement>("container-selected-level");
            _containerSelectedLevel.style.display = DisplayStyle.None;

            _labelSelectedLevel = _containerMain.Q<Label>("label-selected-level");
            _scrollViewSelectedLevel = _containerMain.Q<ScrollView>("scroll-view-selected-level");

            _buttonUnloadAll = _containerMain.Q<Button>("button-unload-all");
            _buttonUnloadAll.clicked += UnloadAllLevels;

            _buttonLoadWorld = _containerMain.Q<Button>("button-load-world");
            _buttonLoadWorld.clicked += LoadAllCurrentWorldLevels;

            _buttonLoadSelection = _containerMain.Q<Button>("button-load-selection");
            _buttonLoadSelection.style.display = DisplayStyle.None;
            _buttonLoadSelection.clicked += LoadCurrentSelection;

            _scrollViewSelectedLevels = _containerMain.Q<ScrollView>("scroll-view-selected-levels");

            _mapView = _containerMain.Q<MapView>("map-view");
            _mapView.SetSelectionAnalysisCallback(OnLevelSelectionChanged);
            _mapView.SetLevelLoadToggleRequestCallback(OnLevelLoadToggleRequest);

            if (Settings.HasLevelEditorScene && Settings.HasCurrentProject && _projects.ContainsKey(Settings.CurrentProject.name))
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
            if (!Settings.HasLevelEditorScene)
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

        private void SelectProject(Project project)
        {
            Settings.CurrentProject = project;
            _projectSelected?.Invoke(project);
        }

        private void OnProjectSelected(Project project)
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

        private void InitializeWorldsDropdown(Project project)
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
            ClearSelectedLevels();
            ClearLoadedLevels();
            Settings.InitializedWorldName = worldName;
            World world = _selectedProjectWorlds[worldName];
            rootVisualElement.schedule.Execute(() => _mapView.InitializeWorld(Settings.CurrentProject, world));
        }

        private void LoadAllCurrentWorldLevels()
        {
            if (!Settings.HasCurrentProject) return;
            if (!Settings.HasInitializedWorldName) return;

            ClearLoadedLevels();

            Bounds bounds = new(new Vector3(0, 0, -10), new Vector3(100, 100, 1));

            foreach (MapLevelElement element in _mapView.LevelElements)
            {
                LoadedLevelEntry entry = LoadLevel(element.Info, false);
                element.RegisterLoadedEntry(entry);

                bounds.min = Vector2.Min(bounds.min, element.Level.UnityWorldRect.min);
                bounds.max = Vector2.Max(bounds.max, element.Level.UnityWorldRect.max);
            }

            SceneView.lastActiveSceneView.Frame(bounds, false);
        }

        #endregion

        #region Loading Levels

        private void OnLevelLoadToggleRequest(MapLevelElement element)
        {
            if (!IsMapSceneOpen()) { return; }

            if (!Settings.IsLevelLoaded(element.Info))
            {
                LoadedLevelEntry entry = LoadLevel(element.Info);
                element.RegisterLoadedEntry(entry);
            }
            else
            {
                UnloadLevel(element.Info);
            }
        }

        private LoadedLevelEntry LoadLevel(LevelInfo levelInfo, bool frameAfterLoad = true)
        {
            LoadedLevelEntry entry;
            if (levelInfo.HasScene)
            {
                string path = AssetDatabase.GUIDToAssetPath(levelInfo.Scene.AssetGuid);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                entry = Settings.RegisterLoadedLevel(levelInfo, scene);

                if (frameAfterLoad)
                {
                    foreach (GameObject obj in scene.GetRootGameObjects())
                    {
                        if (obj.TryGetComponent(out LDtkComponentLevel ldtkComponentLevel))
                        {
                            PrepareLevel(obj, levelInfo);
                            FrameLevel(obj);
                            break;
                        }
                    }
                }
            }
            else
            {
                GameObject obj = Instantiate(levelInfo.Asset) as GameObject;
                obj.name = levelInfo.Name;
                entry = Settings.RegisterLoadedLevel(levelInfo, obj);
                if (frameAfterLoad)
                {
                    PrepareLevel(obj, levelInfo);
                    FrameLevel(obj);
                }
            }
            return entry;
        }

        private void UnloadLevel(LevelInfo levelInfo)
        {
            if (!Settings.TryGetLoadedLevel(levelInfo.Iid, out LoadedLevelEntry loadedLevelEntry)) return;

            Settings.UnregisterLoadedLevel(levelInfo.Iid);

            if (levelInfo.HasScene)
            {
                string path = AssetDatabase.GUIDToAssetPath(levelInfo.Scene.AssetGuid);
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
                UnloadLevel(entry.Info);
            }
        }

        private void OnLevelSelectionChanged(List<ISelectable> selectables)
        {
            ClearSelectedLevels();

            _currentlySelectedLevels = selectables;
            int totalSelected = _currentlySelectedLevels.Count;

            if (totalSelected == 0)
            {
                _containerSelectedLevel.style.display = DisplayStyle.None;
                return;
            }

            if (totalSelected == 1)
            {
                if (selectables[0] is MapLevelElement mapElement)
                {
                    SelectedLevelElement selectedLevelElement = new(mapElement);
                    _scrollViewSelectedLevels.Add(selectedLevelElement);

                    LevelElement levelElement = new(mapElement.Info);
                    _labelSelectedLevel.text = mapElement.Info.Name;
                    _scrollViewSelectedLevel.Add(levelElement);
                }

                _containerSelectedLevel.style.display = DisplayStyle.Flex;
                return;
            }

            foreach (ISelectable selectable in selectables)
            {
                if (selectable is MapLevelElement MapElement)
                {
                    SelectedLevelElement selectedLevelElement = new(MapElement);
                    _scrollViewSelectedLevels.Add(selectedLevelElement);
                }
            }

            _containerSelectedLevel.style.display = DisplayStyle.None;
            _buttonLoadSelection.style.display = DisplayStyle.Flex;
        }

        private void ClearSelectedLevels()
        {
            List<SelectedLevelElement> selectedLevelElements = _scrollViewSelectedLevels.Query<SelectedLevelElement>().ToList();

            foreach (SelectedLevelElement element in selectedLevelElements)
            {
                element.Dismiss();
            }

            _scrollViewSelectedLevels.Clear();
            _scrollViewSelectedLevel.Clear();

            _containerSelectedLevel.style.display = DisplayStyle.None;
            _buttonLoadSelection.style.display = DisplayStyle.None;
        }

        private void LoadCurrentSelection()
        {
            foreach (ISelectable selectable in _currentlySelectedLevels)
            {
                if (selectable is MapLevelElement mapElement)
                {
                    LoadedLevelEntry entry = LoadLevel(mapElement.Info, false);
                    mapElement.RegisterLoadedEntry(entry);
                }
            }
        }

        private void PrepareLevel(GameObject obj, LevelInfo levelInfo)
        {
            if (obj.TryGetComponent(out SceneLevelSetuper setuper))
            {
                setuper.Setup(levelInfo);
            }

            if (obj.TryGetComponent(out LevelBoundaries boundaries))
            {
                boundaries.Compose();
            }
        }

        private void FrameLevel(GameObject obj)
        {
            Selection.activeGameObject = obj;
            SceneView.lastActiveSceneView.FrameSelected();
        }

        private void UnloadAllLevels()
        {
            ClearLoadedLevels();

            // Centering Scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            Bounds bounds = new(new Vector3(0, 0, -10), new Vector3(100, 100, 1));
            SceneView.lastActiveSceneView.Frame(bounds, false);
        }

        #endregion

        #region Editor Scene

        private void OpenMapEditorScene()
        {
            if (!Settings.HasLevelEditorScene) return;

            if (IsMapSceneOpen())
            {
                return;
            }

            try
            {
                string path = AssetDatabase.GetAssetPath(Settings.LevelEditorScene);
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(path);
                }
            }
            catch (System.Exception e)
            {
                LDtkLevelManager.Logger.Exception(e);
            }
        }

        private void OpenUniverseScene()
        {
            if (!Settings.HasUniverseScene) return;

            if (IsUniverseSceneOpen())
            {
                return;
            }

            try
            {
                string path = AssetDatabase.GetAssetPath(Settings.UniverseScene);
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(path);
                }
            }
            catch (System.Exception e)
            {
                LDtkLevelManager.Logger.Exception(e);
            }
        }

        private bool IsMapSceneOpen()
        {
            if (!Settings.HasLevelEditorScene)
            {
                Settings.ResetState();
                return false;
            }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            bool isOpen = activeScene.name == Settings.LevelEditorScene.name;

            if (!isOpen)
            {
                ClearLoadedLevels();
                Settings.ReleaseLevels();
            }

            return isOpen;
        }

        private bool IsUniverseSceneOpen()
        {
            if (!Settings.HasUniverseScene) { return false; }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            bool isOpen = activeScene.name == Settings.UniverseScene.name;
            return isOpen;
        }

        #endregion
    }
}