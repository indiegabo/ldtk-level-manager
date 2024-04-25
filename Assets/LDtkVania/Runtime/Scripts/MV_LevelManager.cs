using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace LDtkVania
{
    public class MV_LevelManager : MonoBehaviour
    {
        #region Static

        private static MV_LevelManager _instance;
        public static MV_LevelManager Instance => _instance;

        #endregion

        #region Inspector        

        [Tooltip("Mark this if you want this object to NOT be destroyed whe a new scene is loaded.")]
        [SerializeField]
        private bool _persistent = true;

        [SerializeField]
        private bool _alertAboutOtherInstances;

        [SerializeField]
        private MV_Project _project;

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

        private Dictionary<MV_Level, MV_LevelBehaviour> _registeredBehaviours = new();
        private Dictionary<MV_Level, GameObject> _loadedObjects = new();
        private Dictionary<MV_Level, SceneInstance> _loadedScenes = new();
        private List<MV_Level> _shouldBeLoaded = new();
        private List<MV_Level> _shouldBeUnloaded = new();

        #endregion

        #region Getters

        public string ConnectionsContainerName => _project.ConnectionsContainerName;
        public string CheckpointsContainerName => _project.CheckpointsContainerName;

        public UnityEvent<MV_Level> LevelPreparedEvent => _levelPreparedEvent;
        public UnityEvent<MV_Level, MV_LevelTrail> LevelEnteredEvent => _levelEnteredEvent;
        public UnityEvent<MV_Level> LevelDeactivatedEvent => _levelExitedEvent;

        #endregion

        #region Behaviour

        public void Awake()
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

            _instance = this;

            if (_persistent)
                DontDestroyOnLoad(gameObject);
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

            _registeredBehaviours.Clear();
            _loadedObjects.Clear();
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

            if (!_registeredBehaviours.ContainsKey(level))
            {
                MV_Logger.Error($"Trying to prepare a non registered level.", this);
                return;
            }

            _currentLevel = level;
            _currentBehaviour = _registeredBehaviours[_currentLevel];

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
            if (!_registeredBehaviours.ContainsKey(level))
            {
                MV_Logger.Error($"Trying to prepare a non registered level.", this);
                return;
            }

            _currentLevel = level;
            _currentBehaviour = _registeredBehaviours[_currentLevel];

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
            if (!_registeredBehaviours.ContainsKey(level))
            {
                MV_Logger.Error($"Trying to prepare a non registered level.", this);
                return;
            }

            _currentLevel = level;
            _currentBehaviour = _registeredBehaviours[_currentLevel];

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

            foreach (var pair in _loadedObjects)
            {
                if (!_shouldBeLoaded.Contains(pair.Key))
                {
                    _shouldBeUnloaded.Add(pair.Key);
                }
            }

            foreach (var pair in _loadedScenes)
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
                if (!TryGetLevel(neighbour.Iid, out MV_Level mvLevel))
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
                levelLoadTasks.Add(LoadAsync(toLoad));
            }

            await Task.WhenAll(levelLoadTasks);
        }

        private async Task LoadAsync(MV_Level level)
        {
            if (level.Scene == null)
            {
                if (_loadedObjects.ContainsKey(level)) return;

                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(level.AddressableKey);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    MV_Logger.Error($"Async operation for level {level.Name} as an object failed.", this);
                    return;
                }

                GameObject loadedObject = Instantiate(handle.Result);
                _loadedObjects.Add(level, loadedObject);
            }
            else
            {
                if (_loadedScenes.ContainsKey(level)) return;
                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(level.SceneAddressableKey, LoadSceneMode.Additive);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    MV_Logger.Error($"Async operation for loading level {level.Name} as a scene failed.", this);
                    return;
                }

                _loadedScenes.Add(level, handle.Result);
            }
        }

        private async Task UnloadAsync(MV_Level level)
        {
            if (_loadedObjects.TryGetValue(level, out GameObject loadedObject))
            {
                _loadedObjects.Remove(level);
                Destroy(loadedObject);
            }

            if (_loadedScenes.TryGetValue(level, out SceneInstance sceneInstance))
            {
                AsyncOperationHandle handle = Addressables.UnloadSceneAsync(sceneInstance);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    MV_Logger.Error($"Async operation for unloading level {level.Name} as a scene failed.", this);
                    return;
                }

                _loadedScenes.Remove(level);
            }
        }

        private async Task UnloadMultipleAsync(List<MV_Level> levelsToUnload)
        {
            List<Task> unloadTasks = new();

            foreach (MV_Level toUnload in levelsToUnload)
            {
                unloadTasks.Add(UnloadAsync(toUnload));
            }

            await Task.WhenAll(unloadTasks);
        }

        private async Task UnloadAllAsync()
        {
            List<Task> tasks = new();

            foreach (var pair in _loadedObjects)
            {
                tasks.Add(UnloadAsync(pair.Key));
            }

            foreach (var pair in _loadedScenes)
            {
                tasks.Add(UnloadAsync(pair.Key));
            }

            await Task.WhenAll(tasks);
        }

        #endregion

        #region Registering Behaviours

        public void RegisterAsBehaviour(string iid, MV_LevelBehaviour behaviour)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Level under LDtk Iid {iid} not found for registering as behaviour", this);
                return;
            }

            if (_registeredBehaviours.ContainsKey(level))
            {
                MV_Logger.Warning($"Level {level.Name}({level.Iid}) already registered as behaviour but is trying to be registered again", this);
                return;
            }

            _registeredBehaviours.Add(level, behaviour);
        }

        public void UnregisterAsBehaviour(string iid)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Level under LDtk Iid {iid} not found for unregistering as behaviour", this);
                return;
            }

            _registeredBehaviours.Remove(level);
        }

        #endregion

        #region Providing Levels

        public bool TryGetLevel(string iid, out MV_Level level)
        {
            return _project.TryGetLevel(iid, out level);
        }

        public MV_Level GetLevel(string iid)
        {
            return _project.GetLevel(iid);
        }

        #endregion
    }
}