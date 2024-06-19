using LDtkUnity;
using LDtkVania;
using LDtkVania.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
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

        public bool TryGetLevelObject(string iid, out GameObject gameObject)
        {
            if (_loadedObjects.ContainsKey(iid))
            {
                gameObject = _loadedObjects[iid];
                return true;
            }
            else
            {
                gameObject = null;
                return false;
            }
        }

        public void RegisterLoadedLevel(string iid, GameObject gameObject)
        {
            if (_loadedObjects.ContainsKey(iid))
            {
                _loadedObjects[iid] = gameObject;
            }
            else
            {
                _loadedObjects.Add(iid, gameObject);
            }
        }

        public void RegisterLoadedLevel(string iid, Scene scene)
        {
            if (_loadedScenes.ContainsKey(iid))
            {
                _loadedScenes[iid] = scene.name;
            }
            else
            {
                _loadedScenes.Add(iid, scene.name);
            }
        }

        public void UnregisterLoadedLevel(string iid)
        {
            _loadedObjects.Remove(iid);
            _loadedScenes.Remove(iid);
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

        public bool IsLevelLoaded(MV_Level mvLevel)
        {
            if (!mvLevel.HasScene)
            {
                return IsLevelObjectLoaded(mvLevel);
            }
            else
            {
                return IsLevelSceneLoaded(mvLevel);
            }
        }

        private bool IsLevelSceneLoaded(MV_Level mvLevel)
        {
            if (_loadedScenes.ContainsKey(mvLevel.Iid))
            {
                if (IsSceneOpen(_loadedScenes[mvLevel.Iid]))
                {
                    return true;
                }
                else
                {
                    _loadedScenes.Remove(mvLevel.Iid);
                    return false;
                }
            }

            return false;
        }

        private bool IsLevelObjectLoaded(MV_Level mvLevel)
        {
            if (_loadedObjects.ContainsKey(mvLevel.Iid))
            {
                if (_loadedObjects[mvLevel.Iid] != null)
                {
                    return true;
                }
                else
                {
                    _loadedObjects.Remove(mvLevel.Iid);
                    return false;
                }
            }

            return false;
        }

        private bool IsSceneOpen(string sceneName)
        {
            Scene sceneToCheck = EditorSceneManager.GetSceneByName(sceneName);
            if (sceneToCheck == null) return false;
            return sceneToCheck.isLoaded;
        }

        [System.Serializable]
        private class ObjectsDictionary : SerializedDictionary<string, GameObject> { }

        [System.Serializable]
        private class ScenesDictionary : SerializedDictionary<string, string> { }
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