using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Linq;
using LDtkUnity;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.ResourceManagement.Exceptions;

namespace LDtkLevelManager
{
    [DefaultExecutionOrder(-1000)]
    public class LevelLoader : MonoBehaviour
    {
        #region Static

        private static LevelLoader _instance;
        public static LevelLoader Instance => _instance;

        #endregion

        #region Inspector        

        [Tooltip("Mark this if you want this object to NOT be destroyed when a new scene is loaded.")]
        [SerializeField]
        private bool _persistent = true;

        [SerializeField]
        private bool _alertAboutOtherInstances;

        [SerializeField]
        private Project _project;

        [SerializeField]
        private LoadingStrategy _loadingStrategy;

        [SerializeField]
        [Min(1)]
        private int _depth = 1;

        [SerializeField]
        private LevelActivatedEvent _levelActivated;

        [SerializeField]
        private LevelDeactivatedEvent _levelDeactivated;

        [SerializeField]
        private LevelPreparedEvent _levelPrepared;

        #endregion

        #region Fields

        private LevelInfo _currentLevel;
        private LevelBehaviour _currentBehaviour;

        private readonly Dictionary<string, LevelBehaviour> _registeredBehaviours = new();

        private readonly Dictionary<string, GameObject> _universeObjects = new();
        private readonly Dictionary<string, GameObject> _standAloneObjects = new();
        private readonly Dictionary<string, SceneInstance> _universeScenes = new();
        private readonly Dictionary<string, SceneInstance> _standAloneScenes = new();

        private readonly HashSet<string> _shouldBeLoaded = new();
        private readonly HashSet<string> _shouldBeUnloaded = new();
        private readonly Queue<(LevelInfo, int)> _neighboursQueue = new();

        #endregion

        #region Getters


        /// <summary>
        /// The current project (<see cref="Project"/>).
        /// </summary>   
        public Project Project => _project;

        /// <summary>
        /// The current loading strategy (<see cref="Strategy"/>) of the LevelManger (<see cref="LevelLoader"/>).
        /// </summary>
        public LoadingStrategy Strategy => _loadingStrategy;

        /// <summary>
        /// The navigation layer's name set in the LDtk project
        /// </summary>
        public string NavigationLayer => _project.NavigationLayer;

        /// <summary>
        /// The current level (<see cref="LevelInfo"/>).
        /// </summary>
        public LevelInfo CurrentLevel => _currentLevel;

        /// <summary>
        /// The current level's behaviour (<see cref="LevelBehaviour"/>).
        /// </summary>
        public LevelBehaviour CurrentBehaviour => _currentBehaviour;

        /// <summary>
        /// The event that is triggered when a level is deactivated.
        /// </summary>
        public LevelDeactivatedEvent LevelDeactivated => _levelDeactivated;

        /// <summary>
        /// The event that is triggered when a level is prepared.
        /// </summary>
        public LevelPreparedEvent LevelPrepared => _levelPrepared;

        /// <summary>
        /// The event that is triggered when a level is activated.
        /// </summary>
        public LevelActivatedEvent LevelActivated => _levelActivated;

        public bool InStandAloneLevel => _currentLevel.StandAlone;

        #endregion

        #region Behaviour

        public virtual void Awake()
        {
            LevelLoader currentInstance = Instance;

            if (currentInstance != null && currentInstance != this)
            {
                if (_alertAboutOtherInstances)
                {
                    Logger.Error($"{name} - Awake interrupted due to other instance being already active.", this);
                }

                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (_persistent)
                DontDestroyOnLoad(gameObject);

            if (!ProjectsService.Instance.TryGetLdtkJson(_project, out LdtkJson ldtkProject))
            {
                Logger.Error($"Failed to load LDtkJson for project {_project.name}.", this);
                return;
            }

            LDtkIidBank.CacheIidData(ldtkProject);
        }

        #endregion

        #region Requests

        /// <summary>
        /// Loads a level by its LDtk Iid. If the level is not present in the project, <br />
        /// an error will be logged and no action will be taken. <br />
        /// <br />
        /// The level will be loaded using the current Loaders's defined strategy (<see cref="LoadingStrategy"/>).
        /// <br />
        /// <b>Strategies:</b>
        /// <list type="bullet">
        /// <item><b>Neighbours:</b> Guarantees the presence of the level and its immediate neighbours. The amount of neighbours is defined by the Loader's <see cref="Depth"/> parameter.</item>
        /// <item><b>Worlds:</b> Guarantees the presence of the level and its entire world. </item>
        /// <item><b>Area:</b> Guarantees the presence of the level and the entire area it is in.</item>
        /// </list>
        /// </summary>
        /// <param name="iid">The LDtk Iid of the level to load.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the level is loaded.</returns>
        public virtual async UniTask LoadUniverseLevel(string iid)
        {
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                Logger.Error($"Level under LDtk Iid {iid} not present in project {_project.name}", this);
                return;
            }

            await LoadUniverseLevel(level);
        }

        /// <summary>
        /// Loads a level by its <see cref="LevelInfo"/>. If the level is not present in the project, <br />
        /// an error will be logged and no action will be taken. <br />
        /// <br />
        /// The level will be loaded using the current Loaders's defined strategy (<see cref="LoadingStrategy"/>).
        /// <br />
        /// <b>Strategies:</b>
        /// <list type="bullet">
        /// <item><b>Neighbours:</b> Guarantees the presence of the level and its immediate neighbours. The amount of neighbours is defined by the Loader's <see cref="Depth"/> parameter.</item>
        /// <item><b>Worlds:</b> Guarantees the presence of the level and its entire world. </item>
        /// <item><b>Area:</b> Guarantees the presence of the level and the entire area it is in.</item>
        /// </list>
        /// </summary>
        /// <param name="level">The <see cref="LevelInfo"/> of the level to load.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the level is loaded.</returns>
        public virtual async UniTask LoadUniverseLevel(LevelInfo level)
        {
            if (level.StandAlone)
            {
                Logger.Error($"Level {level.Iid} is standalone and cannot be loaded as a Universe level.", this);
                return;
            }

            /// Load the level using the current strategy.
            switch (_loadingStrategy)
            {
                /// Load the level by loading its neighboring levels.
                case LoadingStrategy.Neighbours:
                    await LoadNeighboursAsync(level);
                    break;

                /// Load the level by loading its world.
                /// If the current level is not in the same world as the level to load,
                /// the world will be loaded. Otherwise, nothing changes.
                case LoadingStrategy.Worlds:
                    if (_currentLevel == null || _currentLevel.WorldName != level.WorldName)
                    {
                        await LoadUniverseWorld(level.WorldName);
                    }
                    break;

                /// Load the level by loading its area.
                /// If the current level is not in the same area as the level to load,
                /// the area will be loaded. Otherwise, nothing changes.
                case LoadingStrategy.Areas:
                    if (_currentLevel == null || _currentLevel.AreaName != level.AreaName)
                    {
                        await LoadUniverseArea(level.AreaName);
                    }
                    break;
            }
        }

        /// <summary>
        /// Unloads the entire universe.
        /// </summary>
        /// <remarks>
        /// This method will unload all levels that have been loaded as part of the universe.
        /// It will not unload standalone levels.
        /// </remarks>
        /// <returns>A <see cref="UniTask"/> that completes when all levels have been unloaded.</returns>
        public virtual async UniTask UnloadUniverse()
        {
            await UniTask.WhenAll(_universeObjects.Keys.Concat(_universeScenes.Keys).Select(UnloadAsync));
        }

        /// <summary>
        /// Loads a standalone level by its LDtk Iid. If the level is not present in the project, <br />
        /// an error will be logged and no action will be taken. <br />
        /// </summary>
        /// <remarks>
        /// This method will check if the given level is standalone. If it is not, it will not do anything.
        /// </remarks>
        /// <param name="iid">The LDtk Iid of the level to load.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the level is loaded.</returns>
        public virtual async UniTask LoadStandaloneLevel(string iid)
        {
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                Logger.Error($"Level under LDtk Iid {iid} not present in project {_project.name}", this);
                return;
            }

            await LoadStandaloneLevel(level);
        }

        /// <summary>
        /// Loads a standalone level asynchronously.
        /// </summary>
        /// <remarks>
        /// This method will check if the given level is standalone. If it is not, it will not do anything.
        /// </remarks>
        /// <param name="level">The level to load.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the level is loaded.</returns>
        public virtual async UniTask LoadStandaloneLevel(LevelInfo level)
        {
            if (!level.StandAlone)
            {
                Logger.Error($"Level {level.Name} is not standalone.", this);
                return;
            }

            await LoadAsync(level.Iid, true);
        }

        /// <summary>
        /// Unloads a standalone level by its LDtk Iid.
        /// </summary>
        /// <param name="iid">The LDtk Iid of the level to unload.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the level is unloaded.</returns>
        public virtual async UniTask UnloadStandaloneLevel(string iid, bool removeFromRegistry = true)
        {
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                Logger.Error($"Level under LDtk Iid {iid} not present in project {_project.name}", this);
                return;
            }

            await UnloadStandaloneLevel(level, removeFromRegistry);
        }

        /// <summary>
        /// Unloads a standalone level asynchronously.
        /// </summary>
        /// <remarks>
        /// This method will unload the level if it has been loaded as an object or as a scene.
        /// </remarks>
        /// <param name="level">The level to unload.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the unload operation is complete.</returns>
        public virtual async UniTask UnloadStandaloneLevel(LevelInfo level, bool removeFromRegistry = true)
        {
            // Check if the level has been loaded as an object
            if (_standAloneObjects.TryGetValue(level.Iid, out GameObject loadedObject))
            {
                // Remove the level from the list of loaded objects
                if (removeFromRegistry)
                    _standAloneObjects.Remove(level.Iid);

                // Destroy the loaded object
                Destroy(loadedObject);
                return;
            }

            // Check if the level has been loaded as a scene
            if (_standAloneScenes.TryGetValue(level.Iid, out SceneInstance sceneInstance))
            {
                // Start the unload operation
                AsyncOperationHandle handle = Addressables.UnloadSceneAsync(sceneInstance, false);

                // Wait for the unload operation to finish
                await handle;

                // Check if the unload operation succeeded
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    // Log an error if the unload operation failed
                    Logger.Error($"Async operation for unloading level {level.name} as a scene failed.", this);
                    if (handle.OperationException != null)
                        Logger.Exception(handle.OperationException, this);
                    return;
                }

                // Remove the level from the list of loaded scenes
                if (removeFromRegistry)
                    _standAloneScenes.Remove(level.Iid);
            }
        }

        /// <summary>
        /// Unloads all standalone levels.
        /// </summary>
        /// <remarks>
        /// This method will unload all levels that have been loaded as standalone levels.
        /// It will not unload levels that have been loaded as part of the universe.
        /// </remarks>
        /// <returns>A <see cref="UniTask"/> that completes when all levels have been unloaded.</returns>
        public virtual async UniTask UnloadAllStandaloneLevels()
        {

            List<string> keys = _standAloneObjects.Keys.Concat(_standAloneScenes.Keys).ToList();
            List<UniTask> tasks = new List<UniTask>();
            foreach (string key in keys)
            {
                tasks.Add(UnloadStandaloneLevel(key, false));
            }

            await UniTask.WhenAll(tasks);

            _standAloneObjects.Clear();
            _standAloneScenes.Clear();
        }

        /// <summary>
        /// Unloads all loaded levels, both standalone and universe levels.
        /// </summary>
        /// <returns>A <see cref="UniTask"/> that completes when all levels have been unloaded.</returns>
        public virtual async UniTask UnloadEverything()
        {
            await UniTask.WhenAll(
                UnloadAllStandaloneLevels(),
                UnloadUniverse()
            );
        }

        /// <summary>
        /// Unloads all loaded levels and loads all levels of a given world (by name). If the world is not present in the project, <br/>
        /// an error will be logged and no action will be taken.<br/>
        /// <br/>
        /// Your LDtk project must have a world with the given name.
        /// </summary>
        /// <param name="worldName">The name of the world to load.</param>
        /// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
        public virtual async UniTask LoadUniverseWorld(string worldName)
        {
            if (_loadingStrategy != LoadingStrategy.Worlds)
            {
                Logger.Error($"LoadWorld method can only be used with when strategy is set to '{LoadingStrategy.Worlds}'.", this);
                return;
            }

            /// Get all the Iids of the levels in the given world.
            HashSet<string> iids = _project.GetAllLevelsIidsInWorld(worldName);

            if (iids == null || iids.Count == 0)
            {
                Logger.Error($"Trying to load world {worldName} but it has no levels.", this);
                return;
            }

            /// Exit the current level before loading new ones.
            DeactivatePreparedLevel();

            /// Unload all loaded levels and objects before loading new ones.
            await UnloadUniverse();

            /// Clear the lists of registered behaviors and loaded objects and scenes
            /// before loading new levels.
            _registeredBehaviours.Clear();
            _universeObjects.Clear();
            _universeScenes.Clear();

            /// Load all the levels in the given world.
            await LoadMultipleAsync(iids);
        }

        /// <summary>
        /// Unloads all loaded levels and loads all levels of a given area (by name). If the area is not present in the project, <br/>
        /// an error will be logged and no action will be taken.<br/>
        /// <br/>
        /// Your LDtk project must have an area (area enum) with the given name.
        /// </summary>
        /// <param name="worldName">The name of the world to load.</param>
        /// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
        public virtual async UniTask LoadUniverseArea(string areaName)
        {
            if (_loadingStrategy != LoadingStrategy.Areas)
            {
                // LoadArea can only be used when the strategy is set to LevelLoadingStrategy.Areas
                Logger.Error(
                    $"LoadArea method can only be used with when strategy is set to '{LoadingStrategy.Areas}'.",
                    this
                );
                return;
            }

            /// Get all the Iids of the levels in the given area.
            HashSet<string> iids = _project.GetAllLevelsIidsInArea(areaName);

            if (iids == null || iids.Count == 0)
            {
                // If there are no levels in the area, log an error and return.
                Logger.Error($"Trying to load area {areaName} but it has no levels.", this);
                return;
            }

            /// Exit the current level before loading new ones.
            DeactivatePreparedLevel();

            /// Unload all loaded levels and objects before loading new ones.
            await UnloadUniverse();

            /// Clear the lists of registered behaviors and loaded objects and scenes
            /// before loading new levels.
            _registeredBehaviours.Clear();
            _universeObjects.Clear();
            _universeScenes.Clear();

            /// Load all the levels in the given area.
            await LoadMultipleAsync(iids);
        }

        /// <summary>
        /// Prepares the level to be entered. The target MV_LevelBehaviour will <br/>
        /// try finding a spot set as main to place the character into.<br/>
        /// <br/>
        /// <b>Important:</b> This should be called when curtains are closed.<br/>
        /// <br/>
        /// </summary>
        /// <param name="iid">The Iid of the level to prepare.</param>
        public virtual void Prepare(ILevelFlowSubject subject, string iid)
        {
            // If the level could not be found, do not attempt to prepare it.
            if (!EvaluateAndPrepareLevel(iid, out LevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the main spot.
            if (!levelBehaviour.Prepare(subject, out LevelTrail trail)) return;
            _levelPrepared.Invoke(levelBehaviour, subject, trail);
        }

        /// <summary>
        /// Prepares the level to be entered placing the character at the given position.<br/>
        /// <br/>
        /// <b>Important:</b> This should be called when curtains are closed.<br/>
        /// <br/>
        /// </summary>
        /// <param name="iid">The Iid of the level to prepare.</param>
        /// <param name="position">The position to place the character at.</param>
        /// <param name="facingSign">The facing sign of the character.</param>
        public virtual void Prepare(ILevelFlowSubject subject, string iid, Vector2 position, int facingSign)
        {
            // If the level could not be found, do not attempt to prepare it.
            if (!EvaluateAndPrepareLevel(iid, out LevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the spot.
            if (!levelBehaviour.Prepare(subject, position, facingSign, out LevelTrail trail)) return;
            _levelPrepared.Invoke(levelBehaviour, subject, trail);
        }

        /// <summary>
        /// Prepares the level to be entered placing the character at a given spot.<br/>
        /// <br/>
        /// <b>Important:</b> This should be called when curtains are closed.<br/>
        /// <br/>
        /// </summary>
        /// <param name="iid">The Iid of the level to prepare.</param>
        /// <param name="spotIid">The Iid of the spot to use at the character placement.</param>
        public virtual void Prepare(ILevelFlowSubject subject, string iid, string spotIid)
        {
            // If the level could not be found, do not attempt to prepare it.
            if (!EvaluateAndPrepareLevel(iid, out LevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the spot.
            if (!levelBehaviour.Prepare(subject, spotIid, out LevelTrail trail)) return;
            _levelPrepared.Invoke(levelBehaviour, subject, trail);
        }


        /// <summary>
        /// Prepares the level to be entered through a connection.<br/>       
        /// <br/>
        /// <b>Important:</b> This should be called when curtains are closed.<br/>
        /// <br/>
        /// </summary>
        /// <param name="connection">The connection to use to enter the level.</param>
        public virtual void Prepare(ILevelFlowSubject subject, string iid, IConnection connection)
        {
            if (!EvaluateAndPrepareLevel(iid, out LevelBehaviour levelBehaviour))
            {
                // If the level could not be found, do not attempt to prepare it.
                return;
            }

            // Prepare the level for entering through the connection.
            if (!levelBehaviour.Prepare(subject, connection, out LevelTrail trail)) return;
            _levelPrepared.Invoke(levelBehaviour, subject, trail);
        }

        /// <summary>
        /// Prepares the level to be entered through a portal.<br/>
        /// <br/>
        /// <b>Important:</b> This should be called when curtains are closed.<br/>
        /// <br/>
        /// </summary>
        /// <param name="iid">The Iid of the level to prepare.</param>
        /// <param name="portal">The portal to use to enter the level.</param>
        public virtual void Prepare(ILevelFlowSubject subject, string iid, IPortal portal)
        {
            // If the level could not be found, do not attempt to prepare it.
            if (!EvaluateAndPrepareLevel(iid, out LevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the portal.
            if (!levelBehaviour.Prepare(subject, portal, out LevelTrail trail)) return;
            _levelPrepared.Invoke(levelBehaviour, subject, trail);
        }


        /// <summary>
        /// Tries to prepare a level by its Iid.<br/>
        /// <br/>
        /// It first tries to get the level by its Iid from the project.<br/>
        /// If the level is not found, it logs an error and returns false.<br/>
        /// <br/>
        /// If the level is found, it tries to get the registered behaviour for the level.<br/>
        /// If the level is not registered, it logs an error and returns false.<br/>
        /// <br/>
        /// If the level is registered, it prepares the level according to the strategy and returns true.
        /// </summary>
        /// <param name="iid">The Iid of the level to prepare.</param>
        /// <param name="behaviour">The behaviour to be used to prepare the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        protected virtual bool EvaluateAndPrepareLevel(string iid, out LevelBehaviour behaviour)
        {
            // Tries to get a level by its Iid.
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                // If the level is not found, log the error and return false.
                Logger.Error($"Trying to prepare a level by Iid {iid} but it was not found.", this);
                behaviour = null;
                return false;
            }

            // Tries to get the MV_LevelBehaviour for the level.
            if (!_registeredBehaviours.TryGetValue(iid, out behaviour))
            {
                // If the level is not registered, log the error and return false.
                Logger.Error($"Trying to prepare a level by Iid {iid} but it is not registered.", this);
                return false;
            }

            // Prepares the level according to the strategy.

            _currentLevel = level;
            _currentBehaviour = behaviour;

            if (!level.StandAlone && _loadingStrategy == LoadingStrategy.Neighbours)
            {
                _ = LoadNeighboursAsync(_currentLevel);
            }

            // If the level was prepared, return true.
            return true;
        }

        /// <summary>
        /// Enters the current prepared level.<br/>
        /// <br/>
        /// <b>Important:</b> This is the method you call AFTER opening the curtains.<br/>
        /// <br/>        
        /// </summary>
        /// <param name="iid">The Iid of the level to enter.</param>
        public virtual void ActivatePreparedLevel()
        {
            if (_currentLevel == null || _currentBehaviour == null)
            {
                // If the level is not prepared, log the error and return false.
                Logger.Error($"Trying to enter a non prepared level.", this);
                return;
            }

            // Enters the level according to its behaviour.
            _currentBehaviour.Activate();
            // Invokes the level entered event for the current level.
            _levelActivated.Invoke(_currentBehaviour);
        }

        /// <summary>
        /// Exits the current entered level.<br/>
        /// <br/>
        /// <b>Important:</b> This should be called when the curtains are already fully closed<br/>
        /// since all level subjects will receive a level exited event in the same frame.<br/>
        /// <br/>        
        /// </summary>
        public virtual void DeactivatePreparedLevel()
        {
            if (_currentBehaviour != null)
            {
                // Calls the level exited event for the current level.
                _currentBehaviour.Deactivate();

                // Invokes the level exited event for the current level.
                _levelDeactivated.Invoke(_currentBehaviour);
            }
        }

        #endregion

        #region Loading Neighbours

        /// <summary>
        /// This will load all neighbours of the given level up to the defined depth.
        /// </summary>
        /// <param name="level">The level to load neighbours from.</param>
        /// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
        protected virtual async UniTask LoadNeighboursAsync(LevelInfo level)
        {
            // Check if the given level is null
            if (level == null)
            {
                Logger.Error($"Trying to load neighbours for a null level.", this);
                return;
            }

            float depth = Mathf.Clamp(_depth, 1, 10);

            // Clears the queue of levels to be loaded
            _neighboursQueue.Clear();

            // Clear the lists of levels to be loaded and unloaded
            _shouldBeLoaded.Clear();
            _shouldBeUnloaded.Clear();

            // Add the given level to the queue and to the list of levels to be loaded
            _neighboursQueue.Enqueue((level, 0));
            _shouldBeLoaded.Add(level.Iid);

            // While there are levels in the queue
            while (_neighboursQueue.Count > 0)
            {
                // Get the next level and its depth
                (LevelInfo currentLevel, int currentDepth) = _neighboursQueue.Dequeue();

                // If the current depth is less than the given depth
                if (currentDepth < depth)
                {
                    // For each neighbour of the current level
                    foreach (Level neighbour in currentLevel.LDtkLevel.Neighbours)
                    {
                        // Try to get the neighbour level
                        if (!TryGetLevel(neighbour.Iid, out LevelInfo levelInfo))
                        {
                            // If the neighbour level was not found, log an error
                            Logger.Error($"{name} could not find neighbour under Iid {neighbour.Iid} for level {currentLevel.Name}", this);
                            continue;
                        }

                        // If the neighbour level has not been added to the list of levels to be loaded
                        if (!_shouldBeLoaded.Contains(levelInfo.Iid))
                        {
                            // Add the neighbour level to the queue and to the list of levels to be loaded
                            _neighboursQueue.Enqueue((levelInfo, currentDepth + 1));
                            _shouldBeLoaded.Add(levelInfo.Iid);
                        }
                    }
                }
            }

            // For each level that was previously loaded but is not in the list of levels to be loaded
            foreach (string iid in _universeObjects.Keys)
            {
                if (!_shouldBeLoaded.Contains(iid))
                {
                    // Add the level to the list of levels to be unloaded
                    _shouldBeUnloaded.Add(iid);
                }
            }

            // For each scene that was previously loaded but is not in the list of levels to be loaded
            foreach (string iid in _universeScenes.Keys)
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
        protected virtual async UniTask LoadMultipleAsync(HashSet<string> levelsToLoad)
        {
            // Create a list to store the tasks for loading each level
            List<UniTask> levelLoadTasks = new();

            // For each level to load
            foreach (string toLoadiid in levelsToLoad)
            {
                // Add the task for loading the level to the list
                levelLoadTasks.Add(LoadAsync(toLoadiid));
            }

            // Wait for all the tasks to complete
            await UniTask.WhenAll(levelLoadTasks);
        }

        /// <summary>
        /// This will load a level asynchronously.
        /// </summary>
        /// <param name="iid">The IID of the level to load.</param>
        /// <returns></returns>
        protected virtual async UniTask LoadAsync(string iid, bool standalone = false)
        {
            // Check if the level exists
            if (!TryGetLevel(iid, out LevelInfo level)) return;
            await LoadAsync(level, standalone);
        }

        protected virtual async UniTask LoadAsync(LevelInfo level, bool standalone = false)
        {
            // Check if the level should be loaded as an object or a scene
            if (!level.WrappedInScene)
            {
                await LoadLevelObjectAsync(level, standalone);
            }
            else
            {
                await LoadLevelSceneAsync(level, standalone);
            }
        }

        protected virtual async UniTask LoadLevelObjectAsync(LevelInfo level, bool standalone = false)
        {
            // Check if the level has already been loaded
            if (_universeObjects.ContainsKey(level.Iid) || _standAloneObjects.ContainsKey(level.Iid)) return;

            try
            {
                // Load the level as an object
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(
                    level.Address
                );

                await handle;

                // Check if the load operation succeeded
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Logger.Error($"Async operation for level {level.name} as an object failed.", this);
                    if (handle.OperationException != null)
                        Logger.Exception(handle.OperationException, this);
                    return;
                }

                // Instantiate the loaded object
                GameObject loadedObject = Instantiate(handle.Result);

                // Add the loaded object to the list of loaded objects
                if (!standalone)
                {
                    _universeObjects.Add(level.Iid, loadedObject);
                }
                else
                {
                    _standAloneObjects.Add(level.Iid, loadedObject);
                }
            }
            catch (InvalidKeyException e)
            {
                Logger.Error($"Async operation for level {level.name} as an object failed.", this);
                Logger.Exception(e, this);
            }
            catch (OperationException e)
            {
                Logger.Error($"Async operation for loading level {level.name} failed.", this);
                Logger.Exception(e, this);
            }
            catch (Exception e)
            {
                Logger.Error($"Async operation for level {level.name} as an object  failed.", this);
                Logger.Exception(e, this);
            }
        }

        protected virtual async UniTask LoadLevelSceneAsync(LevelInfo level, bool standalone = false)
        {
            // Check if the level has already been loaded
            if (_universeScenes.ContainsKey(level.Iid) || _standAloneScenes.ContainsKey(level.Iid)) return;

            try
            {
                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(
                    level.SceneInfo.AddressableKey,
                    LoadSceneMode.Additive
                );

                await handle;

                // Check if the load operation succeeded
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Logger.Error($"Async operation for loading level {level.name} as a scene failed.", this);
                    if (handle.OperationException != null)
                        Logger.Exception(handle.OperationException, this);
                    return;
                }

                SceneInstance sceneInstance = handle.Result;

                // Add the loaded scene to the list of loaded scenes
                if (!standalone)
                {
                    _universeScenes.Add(level.Iid, sceneInstance);
                }
                else
                {
                    _standAloneScenes.Add(level.Iid, sceneInstance);
                }
            }
            catch (InvalidKeyException e)
            {
                Logger.Error($"Async operation for level {level.name} as a Scene failed.", this);
                Logger.Exception(e, this);
            }
            catch (OperationException e)
            {
                Logger.Error($"Async operation for loading level {level.name} was canceled.", this);
                Logger.Exception(e, this);
            }
            catch (Exception e)
            {
                Logger.Error($"Async operation for level {level.name} as a Scene failed.", this);
                Logger.Exception(e, this);
            }
        }

        /// <summary>
        /// Unload a level asynchronously.
        /// </summary>
        /// <param name="iid">The IID of the level to unload.</param>
        /// <returns>An async task that represents the unload operation.</returns>
        protected virtual async UniTask UnloadAsync(string iid)
        {
            // Check if the level exists
            if (!TryGetLevel(iid, out LevelInfo level)) return;

            // Check if the level has been loaded as an object
            if (_universeObjects.TryGetValue(iid, out GameObject loadedObject))
            {
                // Remove the level from the list of loaded objects
                _universeObjects.Remove(iid);

                // Destroy the loaded object
                Destroy(loadedObject);
                return;
            }

            // Check if the level has been loaded as a scene
            if (_universeScenes.TryGetValue(iid, out SceneInstance sceneInstance))
            {
                // Start the unload operation
                AsyncOperationHandle handle = Addressables.UnloadSceneAsync(sceneInstance, false);

                // Wait for the unload operation to finish
                await handle;

                // Check if the unload operation succeeded
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    // Log an error if the unload operation failed
                    Logger.Error($"Async operation for unloading level {level.name} as a scene failed.", this);
                    if (handle.OperationException != null)
                        Logger.Exception(handle.OperationException, this);
                    return;
                }

                // Remove the level from the list of loaded scenes
                _universeScenes.Remove(iid);
            }
        }

        /// <summary>
        /// Unload multiple levels asynchronously.
        /// </summary>
        /// <param name="levelsToUnload">The set of levels to unload.</param>
        /// <returns>An async task that represents the unload operation.</returns>
        protected virtual async UniTask UnloadMultipleAsync(HashSet<string> levelsToUnload)
        {
            // Create a list of unload tasks
            List<UniTask> unloadTasks = new();

            // Iterate over each level to unload
            foreach (string toUnloadIid in levelsToUnload)
            {
                // Add the unload task to the list
                unloadTasks.Add(UnloadAsync(toUnloadIid));
            }

            // Wait for all unload tasks to finish
            await UniTask.WhenAll(unloadTasks);
        }

        #endregion

        #region Registering Behaviours

        /// <summary>
        /// Registers a level behaviour to be used when transitioning to the level with the given Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to register the behaviour for.</param>
        /// <param name="behaviour">The behaviour to register.</param>
        public virtual void RegisterAsBehaviour(string iid, LevelBehaviour behaviour)
        {
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                Logger.Error($"Level under LDtk Iid {iid} not found for registering as behaviour", this);
                return;
            }

            if (_registeredBehaviours.ContainsKey(iid))
            {
                // If the level is already registered as a behaviour, do not register it again.
                Logger.Warning($"Level {level.Name}({level.Iid}) already registered as behaviour but is trying to be registered again", this);
                return;
            }

            // Add the behaviour to the list of registered behaviours.
            _registeredBehaviours.Add(iid, behaviour);
        }

        /// <summary>
        /// Unregisters a level behaviour from being used when transitioning to the level with the given Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to unregister the behaviour for.</param>
        public virtual void UnregisterAsBehaviour(string iid)
        {
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
        public virtual bool TryGetLevel(string iid, out LevelInfo level)
        {
            // Try to get the level from the project.
            return _project.TryGetLevel(iid, out level);
        }

        /// <summary>
        /// Retrieves a level by its Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to retrieve.</param>
        /// <returns>The retrieved level if successful, or null if not.</returns>
        public virtual LevelInfo GetLevel(string iid)
        {
            // Attempt to get the level from the project.
            return _project.GetLevel(iid);
        }

        #endregion

        #region Events

        [Serializable]
        public class LevelActivatedEvent : UnityEvent<LevelBehaviour> { }
        [Serializable]
        public class LevelDeactivatedEvent : UnityEvent<LevelBehaviour> { }
        [Serializable]
        public class LevelPreparedEvent : UnityEvent<LevelBehaviour, ILevelFlowSubject, LevelTrail> { }

        #endregion

        #region Metadata

        /// <summary>
        /// Represents the different modes of loading a level.
        /// </summary>
        public enum LevelLoadingMode
        {
            /// <summary>
            /// Only load the level without entering it.
            /// </summary>
            LoadOnly,

            /// <summary>
            /// Load the level and enter it.
            /// </summary>
            LoadAndEnter
        }

        /// <summary>
        /// Represents the different strategies for loading levels.
        /// </summary>
        public enum LoadingStrategy
        {
            /// <summary>
            /// Load the level and its neighbours.
            /// </summary>
            [InspectorName("Level and Neighbours")]
            Neighbours,
            /// <summary>
            /// Load all the levels of the entire world.
            /// </summary>
            [InspectorName("Entire Worlds")]
            Worlds,
            /// <summary>
            /// Load all the levels of the same area.
            /// </summary>
            [InspectorName("Areas")]
            Areas,
        }

        #endregion
    }
}