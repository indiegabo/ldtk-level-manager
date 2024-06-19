using LDtkVania;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDtkVaniaEditor
{
    public delegate void UnloadEvent();
    [System.Serializable]
    public class LoadedLevelEntry
    {
        [SerializeField] private GameObject _loadedObject;
        [SerializeField] private string _loadedSceneName;
        [SerializeField] private MV_Level _mvLevel;

        public GameObject LoadedObject => _loadedObject;
        public string LoadedSceneName => _loadedSceneName;
        public MV_Level MVLevel => _mvLevel;
        public UnloadEvent Unloaded;

        public LoadedLevelEntry(MV_Level mvLevel, GameObject loadedObject)
        {
            _mvLevel = mvLevel;
            _loadedObject = loadedObject;
        }

        public LoadedLevelEntry(MV_Level mvLevel, Scene scene)
        {
            _mvLevel = mvLevel;
            _loadedSceneName = scene.name;
        }

        public bool IsLoaded()
        {
            if (!_mvLevel.HasScene)
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