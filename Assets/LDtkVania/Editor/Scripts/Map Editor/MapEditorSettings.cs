using System.Collections.Generic;
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
        [SerializeField] private LoadedLevelsRegistry _loadedLevelsRegistry = new();
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

        public LoadedLevelEntry RegisterLoadedLevel(MV_Level mvLevel, GameObject loadedObject)
        {
            _loadedLevelsRegistry.Remove(mvLevel.Iid);
            LoadedLevelEntry entry = new(mvLevel, loadedObject);
            _loadedLevelsRegistry.Add(mvLevel.Iid, entry);
            return entry;
        }

        public LoadedLevelEntry RegisterLoadedLevel(MV_Level mvLevel, Scene scene)
        {
            _loadedLevelsRegistry.Remove(mvLevel.Iid);
            LoadedLevelEntry entry = new(mvLevel, scene);
            _loadedLevelsRegistry.Add(mvLevel.Iid, entry);
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

        public bool IsLevelLoaded(MV_Level mvLevel)
        {
            if (_loadedLevelsRegistry.TryGetValue(mvLevel.Iid, out LoadedLevelEntry entry))
            {
                bool isLoaded = entry.IsLoaded();
                if (!isLoaded)
                {
                    _loadedLevelsRegistry.Remove(mvLevel.Iid);
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