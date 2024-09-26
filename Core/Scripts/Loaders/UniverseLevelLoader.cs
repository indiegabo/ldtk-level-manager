using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LDtkLevelManager.EventBus;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace LDtkLevelManager
{
    public abstract class UniverseLevelLoader : LevelLoader
    {
        #region Fields

        protected UniverseLevelBehaviour _currentBehaviour;

        protected readonly Dictionary<string, UniverseLevelBehaviour> _registeredBehaviours = new();

        protected readonly Dictionary<string, GameObject> _universeObjects = new();
        protected readonly Dictionary<string, GameObject> _standAloneObjects = new();
        protected readonly Dictionary<string, SceneInstance> _universeScenes = new();
        protected readonly Dictionary<string, SceneInstance> _standAloneScenes = new();

        protected readonly HashSet<string> _shouldBeLoaded = new();
        protected readonly HashSet<string> _shouldBeUnloaded = new();

        protected LevelPreparationEvent _preparationEventData = new();

        #endregion

        #region Getters        

        /// <summary>
        /// The current level (<see cref="LevelInfo"/>).
        /// </summary>
        public LevelInfo CurrentLevel => _currentLevel;

        /// <summary>
        /// The current level's behaviour (<see cref="UniverseLevelBehaviour"/>).
        /// </summary>
        public UniverseLevelBehaviour CurrentBehaviour => _currentBehaviour;


        public bool InStandAloneLevel => _currentLevel.StandAlone;

        #endregion

        #region Abstractions

        protected abstract bool EvaluateAndPrepareLevel(string iid, out UniverseLevelBehaviour behaviour);

        #endregion

        #region Loading and Unloading        

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

        #endregion

        #region Preparing level

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
            if (!EvaluateAndPrepareLevel(iid, out UniverseLevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the main spot.
            if (!levelBehaviour.Prepare(subject, out LevelTrail trail)) return;
            AnnouncePreparation(_currentBehaviour, subject, trail);
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
            if (!EvaluateAndPrepareLevel(iid, out UniverseLevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the spot.
            if (!levelBehaviour.Prepare(subject, position, facingSign, out LevelTrail trail)) return;
            AnnouncePreparation(_currentBehaviour, subject, trail);
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
            if (!EvaluateAndPrepareLevel(iid, out UniverseLevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the spot.
            if (!levelBehaviour.Prepare(subject, spotIid, out LevelTrail trail)) return;
            AnnouncePreparation(_currentBehaviour, subject, trail);
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
            if (!EvaluateAndPrepareLevel(iid, out UniverseLevelBehaviour levelBehaviour))
            {
                // If the level could not be found, do not attempt to prepare it.
                return;
            }

            // Prepare the level for entering through the connection.
            if (!levelBehaviour.Prepare(subject, connection, out LevelTrail trail)) return;
            AnnouncePreparation(_currentBehaviour, subject, trail);
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
            if (!EvaluateAndPrepareLevel(iid, out UniverseLevelBehaviour levelBehaviour))
            {
                return;
            }

            // Prepare the level for entering through the portal.
            if (!levelBehaviour.Prepare(subject, portal, out LevelTrail trail)) return;

            AnnouncePreparation(_currentBehaviour, subject, trail);
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
            AnnounceActivation(_currentBehaviour);
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
            if (_currentBehaviour == null) return;

            // Calls the level exited event for the current level.
            _currentBehaviour.Deactivate();
            AnnounceDeactivation(_currentBehaviour);
        }

        protected virtual void AnnouncePreparation(
            UniverseLevelBehaviour behaviour,
            ILevelFlowSubject subject,
            LevelTrail trail
        )
        {
            _preparationEventData.behaviour = behaviour;
            _preparationEventData.subject = subject;
            _preparationEventData.trail = trail;

            Bus<LevelPreparationEvent>.Raise(_preparationEventData);
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

        #region Registering Behaviours

        /// <summary>
        /// Registers a level behaviour to be used when transitioning to the level with the given Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to register the behaviour for.</param>
        /// <param name="behaviour">The behaviour to register.</param>
        public virtual void RegisterAsBehaviour(string iid, UniverseLevelBehaviour behaviour)
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

        #region Standalone Levels

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
            List<UniTask> tasks = new();
            foreach (string key in keys)
            {
                tasks.Add(UnloadStandaloneLevel(key, false));
            }

            await UniTask.WhenAll(tasks);

            _standAloneObjects.Clear();
            _standAloneScenes.Clear();
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

        #endregion
    }
}