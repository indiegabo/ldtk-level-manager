using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using LDtkUnity;
using System.Linq;
using System;

namespace LDtkVania
{
    [DefaultExecutionOrder(-1000000)]
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
        private MV_LevelManagerStrategy _strategy;

        [SerializeField]
        [Min(1)]
        private int _depth = 1;

        [SerializeField]
        private UnityEvent<MV_Level> _levelPreparedEvent;

        [SerializeField]
        private UnityEvent<MV_Level> _levelEnteredEvent;

        [SerializeField]
        private UnityEvent<MV_Level> _levelExitedEvent;

        #endregion

        #region Fields

        private MV_Level _currentLevel;
        private MV_LevelBehaviour _currentBehaviour;

        private readonly Dictionary<string, MV_LevelBehaviour> _registeredBehaviours = new();
        private readonly Dictionary<string, GameObject> _loadedObjects = new();
        private readonly Dictionary<string, SceneInstance> _loadedScenes = new();
        private readonly HashSet<string> _shouldBeLoaded = new();
        private readonly HashSet<string> _shouldBeUnloaded = new();

        #endregion

        #region Getters

        public MV_LevelManagerStrategy Strategy => _strategy;
        public string NavigationLayer => _project.NavigationLayer;

        public MV_Level CurrentLevel => _currentLevel;
        public MV_LevelBehaviour CurrentBehaviour => _currentBehaviour;

        public UnityEvent<MV_Level> LevelPreparedEvent => _levelPreparedEvent;
        public UnityEvent<MV_Level> LevelEnteredEvent => _levelEnteredEvent;
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

            LDtkIidBank.CacheIidData(_project.LDtkProject);
        }

        #endregion

        #region Entering Levels

        public async Task LoadLevel(string iid)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Level under LDtk Iid {iid} not found while trying a full load", this);
                return;
            }

            switch (_strategy)
            {
                case MV_LevelManagerStrategy.Neighbours:
                    await LoadNeighboursAsync(level);
                    break;
                case MV_LevelManagerStrategy.Worlds:
                    if (_currentLevel == null || _currentLevel.WorldName != level.WorldName)
                    {
                        await LoadWorld(level.WorldName);
                    }
                    break;
                case MV_LevelManagerStrategy.Areas:
                    if (_currentLevel == null || _currentLevel.AreaName != level.AreaName)
                    {
                        await LoadArea(level.AreaName);
                    }
                    break;
            }
        }

        public async Task LoadWorld(string worldName)
        {
            if (!_strategy.Equals(MV_LevelManagerStrategy.Worlds))
            {
                MV_Logger.Error($"LoadWorld method can only be used with when strategy is set to '{MV_LevelManagerStrategy.Worlds}'.", this);
                return;
            }

            HashSet<string> iids = _project.GetAllLevelsIidsInWorld(worldName);

            if (iids == null || iids.Count == 0)
            {
                MV_Logger.Error($"Trying to load world {worldName} but it has no levels.", this);
                return;
            }

            ExitLevel();

            await UnloadAllAsync();

            _registeredBehaviours.Clear();
            _loadedObjects.Clear();
            _loadedScenes.Clear();

            await LoadMultipleAsync(iids);
        }

        public async Task LoadArea(string areaName)
        {
            if (!_strategy.Equals(MV_LevelManagerStrategy.Areas))
            {
                MV_Logger.Error($"LoadArea method can only be used with when strategy is set to '{MV_LevelManagerStrategy.Areas}'.", this);
                return;
            }

            HashSet<string> iids = _project.GetAllLevelsIidsInArea(areaName);

            if (iids == null || iids.Count == 0)
            {
                MV_Logger.Error($"Trying to load area {areaName} but it has no levels.", this);
                return;
            }

            ExitLevel();

            await UnloadAllAsync();

            _registeredBehaviours.Clear();
            _loadedObjects.Clear();
            _loadedScenes.Clear();

            await LoadMultipleAsync(iids);
        }

        public void Prepare(string iid)
        {
            // If the level could not be found, do not attempt to prepare it.
            if (!PerformPreparation(iid, out MV_LevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the spot.
            levelBehaviour.Prepare();
        }

        /// <summary>
        /// Prepares the level to be entered through an spot.
        /// </summary>
        /// <param name="spot">The spot to use to enter the level.</param>
        public void Prepare(string iid, Vector2 position, int facingSign)
        {
            // If the level could not be found, do not attempt to prepare it.
            if (!PerformPreparation(iid, out MV_LevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the spot.
            levelBehaviour.Prepare(position, facingSign);
        }

        public void Prepare(string iid, string spotIid)
        {
            // If the level could not be found, do not attempt to prepare it.
            if (!PerformPreparation(iid, out MV_LevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the spot.
            levelBehaviour.Prepare(spotIid);
        }

        /// <summary>
        /// Prepares the level to be entered through a connection.
        /// </summary>
        /// <param name="connection">The connection to use to enter the level.</param>
        public void Prepare(string iid, IConnection connection)
        {
            if (!PerformPreparation(iid, out MV_LevelBehaviour levelBehaviour))
            {
                // If the level could not be found, do not attempt to prepare it.
                return;
            }

            // Prepare the level for entering through the connection.
            levelBehaviour.Prepare(connection);
        }

        public void Prepare(string iid, IPortal portal)
        {
            if (!PerformPreparation(iid, out MV_LevelBehaviour levelBehaviour))
            {
                // If the level could not be found, do not attempt to prepare it.
                return;
            }

            // Prepare the level for entering through the portal.
            levelBehaviour.Prepare(portal);
        }

        private bool PerformPreparation(string iid, out MV_LevelBehaviour behaviour)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Trying to prepare a level by Iid {iid} but it was not found.", this);
                behaviour = null;
                return false;
            }

            if (!_registeredBehaviours.TryGetValue(iid, out behaviour))
            {
                MV_Logger.Error($"Trying to prepare a level by Iid {iid} but it is not registered.", this);
                return false;
            }

            switch (_strategy)
            {
                case MV_LevelManagerStrategy.Worlds:
                    PrepareLevelInWorld(level, behaviour);
                    break;
                case MV_LevelManagerStrategy.Areas:
                    PrepareLevelInArea(level, behaviour);
                    break;
                case MV_LevelManagerStrategy.Neighbours:
                default:
                    PrepareNeighbouredLevel(level, behaviour);
                    break;
            }

            return true;
        }

        public void EnterLevel()
        {
            if (_currentLevel == null || _currentBehaviour == null)
            {
                MV_Logger.Error($"Trying to enter a non prepared level.", this);
                return;
            }

            _currentBehaviour.Enter();
            _levelEnteredEvent.Invoke(_currentLevel);
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

        private void PrepareNeighbouredLevel(MV_Level level, MV_LevelBehaviour behaviour)
        {
            _currentLevel = level;
            _currentBehaviour = behaviour;
            _ = LoadNeighboursAsync(_currentLevel);

        }

        private void PrepareLevelInWorld(MV_Level level, MV_LevelBehaviour behaviour)
        {
            _currentLevel = level;
            _currentBehaviour = behaviour;
        }

        private void PrepareLevelInArea(MV_Level level, MV_LevelBehaviour behaviour)
        {
            _currentLevel = level;
            _currentBehaviour = behaviour;
        }

        #endregion

        #region Loading Neighbours

        /// <summary>
        /// This will load all neighbours of the given level up to a certain depth.
        /// </summary>
        /// <param name="level">The level to load neighbours from.</param>
        /// <returns></returns>
        private async Task LoadNeighboursAsync(MV_Level level)
        {
            // Check if the given level is null
            if (level == null)
            {
                MV_Logger.Error($"Trying to load neighbours for a null level.", this);
                return;
            }

            // Check if the given depth is negative
            if (_depth <= 0)
            {
                MV_Logger.Error($"Trying to load neighbours for a level with a negative depth.", this);
                return;
            }

            // Create a queue to store the levels to be loaded
            Queue<(MV_Level, int)> queue = new();

            // Clear the lists of levels to be loaded and unloaded
            _shouldBeLoaded.Clear();
            _shouldBeUnloaded.Clear();

            // Add the given level to the queue and to the list of levels to be loaded
            queue.Enqueue((level, 0));
            _shouldBeLoaded.Add(level.Iid);

            // While there are levels in the queue
            while (queue.Count > 0)
            {
                // Get the next level and its depth
                (MV_Level currentLevel, int currentDepth) = queue.Dequeue();

                // If the current depth is less than the given depth
                if (currentDepth < _depth)
                {
                    // For each neighbour of the current level
                    foreach (Level neighbour in currentLevel.LDtkLevel.Neighbours)
                    {
                        // Try to get the neighbour level
                        if (!TryGetLevel(neighbour.Iid, out MV_Level mvLevel))
                        {
                            // If the neighbour level was not found, log an error
                            MV_Logger.Error($"{name} could not find neighbour under Iid {neighbour.Iid} for level {currentLevel.Name}", this);
                            continue;
                        }

                        // If the neighbour level has not been added to the list of levels to be loaded
                        if (!_shouldBeLoaded.Contains(mvLevel.Iid))
                        {
                            // Add the neighbour level to the queue and to the list of levels to be loaded
                            queue.Enqueue((mvLevel, currentDepth + 1));
                            _shouldBeLoaded.Add(mvLevel.Iid);
                        }
                    }
                }
            }

            // For each level that was previously loaded but is not in the list of levels to be loaded
            foreach (string iid in _loadedObjects.Keys)
            {
                if (!_shouldBeLoaded.Contains(iid))
                {
                    // Add the level to the list of levels to be unloaded
                    _shouldBeUnloaded.Add(iid);
                }
            }

            // For each scene that was previously loaded but is not in the list of levels to be loaded
            foreach (string iid in _loadedScenes.Keys)
            {
                if (!_shouldBeLoaded.Contains(iid))
                {
                    // Add the scene to the list of levels to be unloaded
                    _shouldBeUnloaded.Add(iid);
                }
            }

            // Unload all levels that are in the list of levels to be unloaded
            await UnloadMultipleAsync(_shouldBeUnloaded);

            // Load all levels that are in the list of levels to be loaded
            await LoadMultipleAsync(_shouldBeLoaded);
        }

        #endregion

        #region Loading Levels

        /// <summary>
        /// This will load multiple levels asynchronously.
        /// </summary>
        /// <param name="levelsToLoad">The levels to load.</param>
        /// <returns></returns>
        private async Task LoadMultipleAsync(HashSet<string> levelsToLoad)
        {
            // Create a list to store the tasks for loading each level
            List<Task> levelLoadTasks = new();

            // For each level to load
            foreach (string toLoadiid in levelsToLoad)
            {
                // Add the task for loading the level to the list
                levelLoadTasks.Add(LoadAsync(toLoadiid));
            }

            // Wait for all the tasks to complete
            await Task.WhenAll(levelLoadTasks);
        }

        /// <summary>
        /// This will load a level asynchronously.
        /// </summary>
        /// <param name="iid">The IID of the level to load.</param>
        /// <returns></returns>
        private async Task LoadAsync(string iid)
        {
            // Check if the level exists
            if (!TryGetLevel(iid, out MV_Level level)) return;

            // Check if the level should be loaded as an object or a scene
            if (!level.HasScene)
            {
                // Check if the level has already been loaded
                if (_loadedObjects.ContainsKey(iid)) return;

                // Load the level as an object
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(level.Address);
                await handle.Task;

                // Check if the load operation succeeded
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    MV_Logger.Error($"Async operation for level {level.name} as an object failed.", this);
                    return;
                }

                // Instantiate the loaded object
                GameObject loadedObject = Instantiate(handle.Result);
                // Add the loaded object to the list of loaded objects
                _loadedObjects.Add(iid, loadedObject);
            }
            else
            {
                // Check if the level has already been loaded
                if (_loadedScenes.ContainsKey(iid)) return;

                // Load the level as a scene
                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(level.Scene.AddressableKey, LoadSceneMode.Additive);
                await handle.Task;

                // Check if the load operation succeeded
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    MV_Logger.Error($"Async operation for loading level {level.name} as a scene failed.", this);
                    return;
                }

                // Add the loaded scene to the list of loaded scenes
                _loadedScenes.Add(iid, handle.Result);
            }
        }

        /// <summary>
        /// Unload a level asynchronously.
        /// </summary>
        /// <param name="iid">The IID of the level to unload.</param>
        /// <returns>An async task that represents the unload operation.</returns>
        private async Task UnloadAsync(string iid)
        {
            // Check if the level exists
            if (!TryGetLevel(iid, out MV_Level level)) return;

            // Check if the level has been loaded as an object
            if (_loadedObjects.TryGetValue(iid, out GameObject loadedObject))
            {
                // Remove the level from the list of loaded objects
                _loadedObjects.Remove(iid);

                // Destroy the loaded object
                Destroy(loadedObject);
                return;
            }

            // Check if the level has been loaded as a scene
            if (_loadedScenes.TryGetValue(iid, out SceneInstance sceneInstance))
            {
                // Start the unload operation
                AsyncOperationHandle handle = Addressables.UnloadSceneAsync(sceneInstance, false);

                // Wait for the unload operation to finish
                await handle.Task;

                // Check if the unload operation succeeded
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    // Log an error if the unload operation failed
                    MV_Logger.Error($"Async operation for unloading level {level.name} as a scene failed.", this);
                    MV_Logger.Warning($"Handle status: {handle.Status}");
                    MV_Logger.Warning($"{handle.OperationException?.StackTrace}");
                    return;
                }

                // Remove the level from the list of loaded scenes
                _loadedScenes.Remove(iid);
            }
        }

        /// <summary>
        /// Unload multiple levels asynchronously.
        /// </summary>
        /// <param name="levelsToUnload">The set of levels to unload.</param>
        /// <returns>An async task that represents the unload operation.</returns>
        private async Task UnloadMultipleAsync(HashSet<string> levelsToUnload)
        {
            // Create a list of unload tasks
            List<Task> unloadTasks = new();

            // Iterate over each level to unload
            foreach (string toUnloadIid in levelsToUnload)
            {
                // Add the unload task to the list
                unloadTasks.Add(UnloadAsync(toUnloadIid));
            }

            // Wait for all unload tasks to finish
            await Task.WhenAll(unloadTasks);
        }

        /// <summary>
        /// Unload all levels that have been loaded by this level manager.
        /// </summary>
        /// <returns>An async task that represents the unload operation.</returns>
        private async Task UnloadAllAsync()
        {
            // Create a list of unload tasks
            List<Task> tasks = new();

            List<GameObject> objectsToUnload = _loadedObjects.Values.ToList();
            List<SceneInstance> scenesToUnload = _loadedScenes.Values.ToList();
            _loadedObjects.Clear();
            _loadedScenes.Clear();

            // Iterate over each level that has been loaded as an object
            foreach (GameObject levelObject in objectsToUnload)
            {
                Destroy(levelObject);
            }

            // Iterate over each level that has been loaded as a scene
            foreach (SceneInstance sceneInstance in scenesToUnload)
            {
                // Start the unload operation
                AsyncOperationHandle handle = Addressables.UnloadSceneAsync(sceneInstance, false);
                tasks.Add(handle.Task);
            }

            // Wait for all unload tasks to finish
            await Task.WhenAll(tasks);
        }

        #endregion

        #region Registering Behaviours

        /// <summary>
        /// Registers a level behaviour to be used when transitioning to the level with the given Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to register the behaviour for.</param>
        /// <param name="behaviour">The behaviour to register.</param>
        public void RegisterAsBehaviour(string iid, MV_LevelBehaviour behaviour)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Level under LDtk Iid {iid} not found for registering as behaviour", this);
                return;
            }

            if (_registeredBehaviours.ContainsKey(iid))
            {
                // If the level is already registered as a behaviour, do not register it again.
                MV_Logger.Warning($"Level {level.Name}({level.Iid}) already registered as behaviour but is trying to be registered again", this);
                return;
            }

            // Add the behaviour to the list of registered behaviours.
            _registeredBehaviours.Add(iid, behaviour);
        }

        /// <summary>
        /// Unregisters a level behaviour from being used when transitioning to the level with the given Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to unregister the behaviour for.</param>
        public void UnregisterAsBehaviour(string iid)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                // If the level is not found, log an error.
                MV_Logger.Error($"Level under LDtk Iid {iid} not found for unregistering as behaviour", this);
                return;
            }

            // Remove the behaviour from the list of registered behaviours.
            _registeredBehaviours.Remove(iid);
        }

        #endregion

        #region Providing Levels

        /// <summary>
        /// Attempts to retrieve a level by its Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to retrieve.</param>
        /// <param name="level">The retrieved level if successful, or null if not.</param>
        /// <returns>true if the level was successfully retrieved, false otherwise.</returns>
        public bool TryGetLevel(string iid, out MV_Level level)
        {
            // Try to get the level from the project.
            return _project.TryGetLevel(iid, out level);
        }

        /// <summary>
        /// Retrieves a level by its Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to retrieve.</param>
        /// <returns>The retrieved level if successful, or null if not.</returns>
        public MV_Level GetLevel(string iid)
        {
            // Attempt to get the level from the project.
            return _project.GetLevel(iid);
        }

        #endregion
    }
}