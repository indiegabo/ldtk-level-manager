using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using LDtkVania;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using LDtkUnity;
using UnityEngine.SceneManagement;

namespace LDtkVaniaEditor
{
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

        private static List<MV_Project> GetProjects()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(MV_Project)}");

            List<MV_Project> projects = new();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MV_Project project = AssetDatabase.LoadAssetAtPath<MV_Project>(path);
                if (project != null)
                {
                    projects.Add(project);
                }
            }

            return projects;
        }

        #endregion

        #region Fields

        private Dictionary<string, MV_Project> _projects;
        private MV_Project _selectedProject;
        private Dictionary<string, World> _selectedProjectWorlds;
        private World _selectedWorld;

        private Dictionary<string, GameObject> _loadedObjects;
        private Dictionary<string, Scene> _loadedScenes;
        private List<VisualElement> _levelImages;

        private TemplateContainer _containerMain;

        private DropdownField _dropdownProject;
        private DropdownField _dropdownWorld;
        private Button _buttonOpenScene;
        private Button _buttonClear;
        private VisualElement _containerMapView;

        #endregion

        #region GUI

        public void CreateGUI()
        {
            _projects = new();
            _selectedProjectWorlds = new();
            _loadedObjects = new();
            _loadedScenes = new();
            _levelImages = new();

            VisualElement root = rootVisualElement;

            _containerMain = Resources.Load<VisualTreeAsset>($"UXML/{TemplateName}").Instantiate();
            _dropdownProject = _containerMain.Q<DropdownField>("dropdown-project");
            _dropdownWorld = _containerMain.Q<DropdownField>("dropdown-world");

            _buttonOpenScene = _containerMain.Q<Button>("button-open-scene");
            _buttonOpenScene.clicked += () => OpenMapEditorScene();

            _buttonClear = _containerMain.Q<Button>("button-clear");
            _buttonClear.clicked += () => ClearLevels();

            _containerMapView = _containerMain.Q<VisualElement>("container-map-view");

            InitializeProjectsDropdown();

            root.Add(_containerMain);

            Debug.Log("Map editor window created");
        }

        private void OnDestroy()
        {
            // ClearLevels();
        }

        #endregion

        #region Projects

        private void InitializeProjectsDropdown()
        {
            List<MV_Project> projects = GetProjects();

            foreach (MV_Project project in projects)
            {
                _projects.Add(project.name, project);
            }

            _dropdownProject.choices = projects.Select(x => x.name).ToList();
            _dropdownProject.RegisterValueChangedCallback(x => SelectProject(x.newValue));

            if (_projects.Count > 0)
            {
                _dropdownProject.value = projects[0].name;
                SelectProject(_dropdownProject.value);
            }
        }

        private void SelectProject(string projectName)
        {
            SelectProject(_projects[projectName]);
        }

        private void SelectProject(MV_Project project)
        {
            _selectedProject = project;
            InitializeWorldsDropdown(_selectedProject);
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

            if (_selectedProjectWorlds.Count > 0)
            {
                _dropdownWorld.value = worlds[0].Identifier;
                SelectWorld(_dropdownWorld.value);
            }
        }

        private void SelectWorld(string worldName)
        {
            ClearLevels();
            _levelImages.Clear();
            _containerMapView.Clear();

            _selectedWorld = _selectedProjectWorlds[worldName];

            if (_selectedWorld.WorldLayout != WorldLayout.GridVania)
            {
                Debug.LogWarning("Only grid-based worlds are supported.");
                return;
            }

            foreach (Level level in _selectedWorld.Levels)
            {
                _selectedProject.TryGetLevel(level.Iid, out MV_Level mvLevel);

                VisualElement levelImage = new();

                levelImage.style.position = Position.Absolute;
                levelImage.style.width = level.UnityWorldRect.width * 0.1f;
                levelImage.style.height = level.UnityWorldRect.height * 0.1f;
                levelImage.style.left = level.UnityWorldRect.x * 0.1f;
                levelImage.style.bottom = level.UnityWorldRect.y * 0.1f;
                levelImage.style.backgroundImage = Resources.Load<Sprite>("world-tile").texture;

                _containerMapView.Add(levelImage);
                _levelImages.Add(levelImage);

                if (mvLevel.HasScene && !_loadedScenes.ContainsKey(mvLevel.Iid))
                {
                    string path = AssetDatabase.GUIDToAssetPath(mvLevel.Scene.AssetGuid);
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    _loadedScenes.Add(mvLevel.Iid, scene);
                }
                else if (!_loadedObjects.ContainsKey(mvLevel.Iid))
                {
                    GameObject obj = Instantiate(mvLevel.Asset) as GameObject;
                    obj.name = mvLevel.Name;
                    _loadedObjects.Add(mvLevel.Iid, obj);
                }
            }
        }

        #endregion

        #region Loading Levels

        private void LoadLevel(string iid)
        {

        }

        private void ClearLevels()
        {
            foreach (GameObject obj in _loadedObjects.Values)
            {
                DestroyImmediate(obj);
            }

            foreach (Scene scene in _loadedScenes.Values)
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            _loadedObjects.Clear();
            _loadedScenes.Clear();
        }

        #endregion

        #region Editor Scene

        private bool OpenMapEditorScene()
        {
            if (!_selectedProject || _selectedProject.MapEditorScene == null) return false;

            try
            {
                string path = AssetDatabase.GetAssetPath(_selectedProject.MapEditorScene);
                EditorSceneManager.OpenScene(path);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        #endregion
    }
}