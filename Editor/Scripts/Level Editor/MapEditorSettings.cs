using System.Collections.Generic;
using LDtkUnity;
using LDtkLevelManager;
using LDtkLevelManager.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDtkLevelManagerEditor
{
    [FilePath("LDtkLevelManager/MapEditorSettings", FilePathAttribute.Location.PreferencesFolder)]
    public class MapEditorSettings : ScriptableSingleton<MapEditorSettings>
    {
        [SerializeField] private SceneAsset _levelEditorScene;
        [SerializeField] private SceneAsset _universeScene;
        [SerializeField] private Project _currentProject;
        [SerializeField] private string _currentWorldName;
        [SerializeField] private LoadedLevelsRegistry _loadedLevelsRegistry = new();
        [SerializeField] private MapViewTransform _mapViewTransform;

        public bool HasLevelEditorScene => _levelEditorScene != null;
        public SceneAsset LevelEditorScene
        {
            get => _levelEditorScene;
            set
            {
                _levelEditorScene = value;
                Save(true);
            }
        }

        public bool HasUniverseScene => _universeScene != null;
        public SceneAsset UniverseScene
        {
            get => _universeScene;
            set
            {
                _universeScene = value;
                Save(true);
            }
        }

        public bool HasCurrentProject => _currentProject != null;
        public Project CurrentProject
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
            _currentProject = null;
            _currentWorldName = null;
            ReleaseLevels(false);
            _mapViewTransform = default;
            Save(true);
        }

        public List<LoadedLevelEntry> GetLoadedLevels()
        {
            return new List<LoadedLevelEntry>(_loadedLevelsRegistry.Values);
        }

        public bool TryGetLoadedLevel(string iid, out LoadedLevelEntry entry)
        {
            if (_loadedLevelsRegistry.TryGetValue(iid, out entry))
            {
                bool isLoaded = entry.IsLoaded();
                if (!isLoaded)
                {
                    UnregisterLoadedLevel(iid);
                }
                return isLoaded;
            }
            else
            {
                return false;
            }
        }

        public LoadedLevelEntry RegisterLoadedLevel(LDtkLevelManager.LevelInfo levelInfo, GameObject loadedObject)
        {
            _loadedLevelsRegistry.Remove(levelInfo.Iid);
            LoadedLevelEntry entry = new(levelInfo, loadedObject);
            _loadedLevelsRegistry.Add(levelInfo.Iid, entry);
            return entry;
        }

        public LoadedLevelEntry RegisterLoadedLevel(LDtkLevelManager.LevelInfo levelInfo, Scene scene)
        {
            _loadedLevelsRegistry.Remove(levelInfo.Iid);
            LoadedLevelEntry entry = new(levelInfo, scene);
            _loadedLevelsRegistry.Add(levelInfo.Iid, entry);
            return entry;
        }

        public void UnregisterLoadedLevel(string iid)
        {
            if (_loadedLevelsRegistry.TryGetValue(iid, out LoadedLevelEntry entry))
            {
                entry.Unload();
            }

            _loadedLevelsRegistry.Remove(iid);
        }

        public void ReleaseLevels(bool save = true)
        {
            foreach (LoadedLevelEntry entry in _loadedLevelsRegistry.Values)
            {
                entry.Unload();
            }

            _loadedLevelsRegistry.Clear();

            if (save)
            {
                Save(true);
            }

        }

        public bool IsLevelLoaded(LDtkLevelManager.LevelInfo levelInfo)
        {
            if (_loadedLevelsRegistry.TryGetValue(levelInfo.Iid, out LoadedLevelEntry entry))
            {
                bool isLoaded = entry.IsLoaded();
                if (!isLoaded)
                {
                    _loadedLevelsRegistry.Remove(levelInfo.Iid);
                }
                return isLoaded;
            }
            else
            {
                return false;
            }
        }

        [System.Serializable]
        private class LoadedLevelsRegistry : SerializedDictionary<string, LoadedLevelEntry> { }
    }
}