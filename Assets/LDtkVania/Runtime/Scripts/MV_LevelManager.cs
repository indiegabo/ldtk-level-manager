using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LDtkVania
{
    public class MV_LevelManager : MonoBehaviour
    {
        #region Static

        public static MV_LevelManager Instance { get; protected set; }

        public static bool InstanceUnavailable
        {
            get
            {
                bool unavailable = Instance == null;

                if (unavailable)
                {
                    MV_Logger.Warning($"{nameof(MV_LevelManager)} just failed a Singleton instance validation");
                }

                return unavailable;
            }
        }

        #endregion

        #region Inspector        

        [Tooltip("Mark this if you want this object to NOT be destroyed whe a new scene is loaded.")]
        [SerializeField]
        private bool _persistent = true;

        [SerializeField]
        private bool _alertAboutOtherInstances;

        [SerializeField]
        [Min(1)]
        private int _depth = 1;

        [SerializeField]
        private UnityEvent<MV_Level> _levelPreparedEvent;

        [SerializeField]
        private UnityEvent<MV_Level, MV_LevelTrail> _levelEnteredEvent;

        [SerializeField]
        private UnityEvent<MV_Level> _levelExitedEvent;

        #endregion

        #region Fields

        private MV_Level _currentLevel;
        private MV_LevelBehaviour _currentBehaviour;

        private Dictionary<MV_Level, MV_LevelBehaviour> _loadedLevels;
        private Dictionary<MV_Level, List<SceneField>> _loadedScenes;
        private List<MV_Level> _shouldBeLoaded;
        private List<MV_Level> _shouldBeUnloaded;

        #endregion

        #region Getters

        public UnityEvent<MV_Level> LevelPreparedEvent => _levelPreparedEvent;
        public UnityEvent<MV_Level, MV_LevelTrail> LevelEnteredEvent => _levelEnteredEvent;
        public UnityEvent<MV_Level> LevelDeactivatedEvent => _levelExitedEvent;

        #endregion

        #region Behaviour

        public void Initialize()
        {
            MV_LevelManager currentInstance = Instance;

            if (currentInstance != null && currentInstance != this)
            {
                if (_alertAboutOtherInstances)
                {
                    MV_Logger.Error($"{name} - Awake interrupted due to other instance being already active.", this);
                }

                Destroy(gameObject);

                return;
            }

            Instance = this;

            if (_persistent)
                DontDestroyOnLoad(gameObject);

            _loadedLevels = new Dictionary<MV_Level, MV_LevelBehaviour>();
            _loadedScenes = new Dictionary<MV_Level, List<SceneField>>();
            _shouldBeLoaded = new List<MV_Level>();
            _shouldBeUnloaded = new List<MV_Level>();
        }

        #endregion

        #region Entering Levels

        /// <summary>
        /// This should be used when the target level has no neighbours already loaded 
        /// and therefore needs to await all of them being loaded before 
        /// activating the current level.
        /// 
        /// This also unloads all loaded levels before loading the target level and 
        /// its neighbours
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public async Task FullLevelLoad(MV_Level level, MV_LevelLoadMode mode = MV_LevelLoadMode.LoadOnly)
        {
            ExitLevel();

            await UnloadAllAsync();

            _loadedLevels.Clear();
            _loadedScenes.Clear();

            await LoadLevelAndNeighboursAsync(level);

            if (!mode.Equals(MV_LevelLoadMode.LoadAndEnter)) return;

            await Prepare(level, loadNeighbours: false); // Neighbours already fully loaded.
            EnterLevel();
        }

        /// <summary>
        /// This should be used while transitioning from one level to another.
        /// Have in mind that the target level should be already loaded.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public async Task Prepare(MV_Level level, bool loadNeighbours = true)
        {
            if (string.IsNullOrEmpty(level.Iid))
            {
                MV_Logger.Error($"Null or Empty level Iid.", this);
                return;
            }

            if (!_loadedLevels.ContainsKey(level))
            {
                MV_Logger.Error($"Trying to prepare a non registered level.", this);
                return;
            }

            _currentLevel = level;
            _currentBehaviour = _loadedLevels[_currentLevel];

            if (loadNeighbours)
                await LoadLevelAndNeighboursAsync(_currentLevel);

            await _currentBehaviour.Prepare();
        }

        /// <summary>
        /// This should be used while transitioning from one level to another.
        /// Have in mind that the target level should be already loaded.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public async Task PrepareLevel(MV_Level level, MV_LevelTrail trail, bool loadNeighbours = true)
        {
            if (!_loadedLevels.ContainsKey(level))
            {
                MV_Logger.Error($"Trying to prepare a non registered level.", this);
                return;
            }

            _currentLevel = level;
            _currentBehaviour = _loadedLevels[_currentLevel];

            if (loadNeighbours)
                await LoadLevelAndNeighboursAsync(_currentLevel);

            await _currentBehaviour.Prepare(trail);
        }

        /// <summary>
        /// This should be used while transitioning from one level to another.
        /// Have in mind that the target level should be already loaded.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public async Task PrepareLevel(MV_Level level, MV_ICheckpoint checkpointData, bool loadNeighbours = true)
        {
            if (!_loadedLevels.ContainsKey(level))
            {
                MV_Logger.Error($"Trying to prepare a non registered level.", this);
                return;
            }

            _currentLevel = level;
            _currentBehaviour = _loadedLevels[_currentLevel];

            if (loadNeighbours)
                await LoadLevelAndNeighboursAsync(_currentLevel);

            await _currentBehaviour.Prepare(checkpointData);
        }


        public void EnterLevel()
        {
            if (_currentLevel == null || _currentBehaviour == null)
            {
                MV_Logger.Error($"Trying to enter a non prepared level.", this);
                return;
            }

            MV_LevelTrail trail = _currentBehaviour.Enter();
            // Debug.Log(trail.spawnPosition);
            // Debug.Log(trail.connectionKey);
            _levelEnteredEvent.Invoke(_currentLevel, trail);
        }

        public void ExitLevel()
        {
            if (_currentBehaviour != null)
            {
                _currentBehaviour.Exit();

                if (_currentLevel != null)
                    _levelExitedEvent.Invoke(_currentLevel);
            }
        }

        #endregion

        #region Loading Neighbours

        private async Task LoadLevelAndNeighboursAsync(MV_Level level)
        {
            _shouldBeLoaded.Clear();
            _shouldBeUnloaded.Clear();

            _shouldBeLoaded.Add(level); // Must put current level as first of the list

            for (int i = 0; i < _depth; i++)
            {
                List<MV_Level> neighbours = new();

                foreach (MV_Level neighbourLevel in _shouldBeLoaded)
                {
                    AddNeighboursToList(neighbours, neighbourLevel);
                }

                foreach (MV_Level neighbour in neighbours)
                {
                    if (_shouldBeLoaded.Contains(neighbour)) continue;
                    _shouldBeLoaded.Add(neighbour);
                }
            }

            foreach (var pair in _loadedLevels)
            {
                if (!_shouldBeLoaded.Contains(pair.Key))
                {
                    _shouldBeUnloaded.Add(pair.Key);
                }
            }

            await UnloadMultipleAsync(_shouldBeUnloaded);
            await LoadMultipleAsync(_shouldBeLoaded);
        }

        private void AddNeighboursToList(List<MV_Level> list, MV_Level metroidvaniaLevel)
        {
            foreach (LDtkUnity.Level neighbour in metroidvaniaLevel.LDtkLevel.Neighbours)
            {
                if (!MV_Project.Instance.TryGetLevel(neighbour.Iid, out MV_Level mvLevel))
                {
                    MV_Logger.Error($"{name} could not find neighbour {neighbour.Iid}", this);
                    continue;
                }

                if (list.Contains(mvLevel)) continue;
                list.Add(mvLevel);
            }
        }

        #endregion

        #region Loading Levels

        private async Task LoadMultipleAsync(List<MV_Level> levelsToLoad)
        {
            List<Task> levelLoadTasks = new();

            foreach (MV_Level toLoad in levelsToLoad)
            {
                if (_loadedLevels.ContainsKey(toLoad)) continue;
                levelLoadTasks.Add(LoadAsync(toLoad));
            }

            await Task.WhenAll(levelLoadTasks);
        }

        private async Task LoadAsync(MV_Level level)
        {
            if (_loadedLevels.ContainsKey(level)) return;

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(level.AddressableKey);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                MV_Logger.Error($"Async operation for level level ({level.Iid}) failed.", this);
                return;
            }

            GameObject obj = Instantiate(handle.Result);

            if (!obj.TryGetComponent(out MV_LevelBehaviour behaviour))
            {
                MV_Logger.Error($"Loaded asset {obj.name} for level ({level.Iid}) has no {nameof(MV_LevelBehaviour)} component.", this);
                Destroy(obj);
                return;
            }

            if (level.Scenes != null && level.Scenes.Count > 0)
            {
                List<Task> tasks = new();

                for (int i = 0; i < level.Scenes.Count; i++)
                {
                    string scene = level.Scenes[i];
                    AsyncOperation operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

                    tasks.Add(operation.AwaitAsync());
                }

                _loadedScenes.Add(level, level.Scenes);
            }

            _loadedLevels.Add(level, behaviour);
        }

        private async Task UnloadAsync(MV_Level level)
        {
            if (!_loadedLevels.TryGetValue(level, out MV_LevelBehaviour behaviour)) return;

            List<Task> tasks = new();

            if (_loadedScenes.TryGetValue(level, out List<SceneField> scenes))
            {
                for (int i = 0; i < scenes.Count; i++)
                {
                    string scene = scenes[i];
                    AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);
                    tasks.Add(operation.AwaitAsync());
                }
            }

            await Task.WhenAll(tasks);

            _loadedLevels.Remove(level);
            _loadedScenes.Remove(level);

            Destroy(behaviour.gameObject);
        }

        private async Task UnloadMultipleAsync(List<MV_Level> levelsToUnload)
        {
            List<Task> unloadTasks = new();

            foreach (MV_Level toUnload in levelsToUnload)
            {
                if (!_loadedLevels.ContainsKey(toUnload)) continue;
                unloadTasks.Add(UnloadAsync(toUnload));
            }

            await Task.WhenAll(unloadTasks);
        }

        private async Task UnloadAllAsync()
        {
            List<Task> tasks = new();
            foreach (var pair in _loadedLevels)
            {
                tasks.Add(UnloadAsync(pair.Key));
            }

            await Task.WhenAll(tasks);
        }

        #endregion
    }
}