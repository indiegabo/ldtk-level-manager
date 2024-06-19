using LDtkUnity;
using LDtkVania;
using LDtkVania.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDtkVaniaEditor
{
    [FilePath("LDtkVania/MapEditorSettings", FilePathAttribute.Location.PreferencesFolder)]
    public class MapEditorSettings : ScriptableSingleton<MapEditorSettings>
    {
        [SerializeField] private SceneAsset _mapScene;
        [SerializeField] private MV_Project _currentProject;
        [SerializeField] private string _currentWorldName;
        [SerializeField] private ObjectsDictionary _loadedObjects = new();
        [SerializeField] private ScenesDictionary _loadedScenes = new();
        [SerializeField] private MapViewTransform _mapViewTransform;

        public bool HasMapScene => _mapScene != null;
        public SceneAsset MapScene
        {
            get => _mapScene;
            set
            {
                _mapScene = value;
                Save(true);
            }
        }

        public bool HasCurrentProject => _currentProject != null;
        public MV_Project CurrentProject
        {
            get => _currentProject;
            set
            {
                _currentProject = value;
                Save(true);
            }
        }

        public MapViewTransform MapViewTransform
        {
            get => _mapViewTransform;
            set
            {
                _mapViewTransform = value;
                Save(true);
            }
        }

        public bool HasInitializedWorldName => !string.IsNullOrEmpty(_currentWorldName);
        public string InitializedWorldName
        {
            get => _currentWorldName;
            set
            {
                _currentWorldName = value;
                Save(true);
            }
        }

        public void ResetState()
        {
            ReleaseLevels(false);
            _mapViewTransform = default;
            Save(true);
        }

        public void ReleaseLevels(bool save = true)
        {
            _loadedObjects.Clear();
            _loadedScenes.Clear();

            if (save)
            {
                Save(true);
            }
        }

        [System.Serializable]
        private class ObjectsDictionary : SerializedDictionary<string, Object> { }

        [System.Serializable]
        private class ScenesDictionary : SerializedDictionary<string, Scene> { }
    }
}

// static class MySingletonMenuItems
// {
//     [MenuItem("SingletonTest/Log")]
//     static void LogMySingletonState()
//     {
//         MaptEditorSettings.instance.Log();
//     }

//     [MenuItem("SingletonTest/Modify")]
//     static void ModifyMySingletonState()
//     {
//         MaptEditorSettings.instance.Modify();
//     }
// }