using LDtkLevelManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDtkLevelManagerEditor
{
    public delegate void UnloadEvent();
    [System.Serializable]
    public class LoadedLevelEntry
    {
        [SerializeField] private GameObject _loadedObject;
        [SerializeField] private string _loadedSceneName;
        [SerializeField] private LevelInfo _levelInfo;

        public GameObject LoadedObject => _loadedObject;
        public string LoadedSceneName => _loadedSceneName;
        public LevelInfo Info => _levelInfo;
        public UnloadEvent Unloaded;

        public LoadedLevelEntry(LevelInfo levelInfo, GameObject loadedObject)
        {
            _levelInfo = levelInfo;
            _loadedObject = loadedObject;
        }

        public LoadedLevelEntry(LevelInfo levelInfo, Scene scene)
        {
            _levelInfo = levelInfo;
            _loadedSceneName = scene.name;
        }

        public bool IsLoaded()
        {
            if (!_levelInfo.WrappedInScene)
            {
                return _loadedObject != null;
            }
            else
            {
                return IsSceneOpen(_loadedSceneName);
            }
        }

        private bool IsSceneOpen(string sceneName)
        {
            Scene sceneToCheck = EditorSceneManager.GetSceneByName(sceneName);
            if (sceneToCheck == null) return false;
            return sceneToCheck.isLoaded;
        }

        public void Unload()
        {
            Unloaded?.Invoke();
        }
    }
}