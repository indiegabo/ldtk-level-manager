using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LDtkLevelManager.EventBus;
using LDtkUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace LDtkLevelManager
{
    public abstract class LevelLoader : MonoBehaviour
    {
        #region Static

        private static Dictionary<string, LevelLoader> _loaders = new();

        public static LevelLoader For(Project project)
        {
            if (!_loaders.TryGetValue(project.Iid, out LevelLoader loader))
            {
                throw new ArgumentException("LevelLoader not found for project: " + project.name);
            }

            return loader;
        }

        public static LevelLoader InstantiateLoader(Project project)
        {
            if (_loaders.ContainsKey(project.Iid))
            {
                throw new System.InvalidOperationException("LevelLoader already exists for project: " + project.name);
            }

            var organization = project.Organization;

            var loader = organization switch
            {
                Project.LevelsOrganization.Unrelated => InstantiateUnrelatedLevelsLoader(),
                Project.LevelsOrganization.Connected => InstantiateConnectedLevelsLoader(project.Strategy),
                _ => throw new System.InvalidOperationException("Unrecognized organization: " + organization),
            };

            loader.Initialize(project);
            _loaders.Add(project.Iid, loader);

            return loader;
        }

        public static void ClearLoaders()
        {
            _loaders.Clear();
        }

        private static LevelLoader InstantiateUnrelatedLevelsLoader()
        {
            return new GameObject().AddComponent<UnrelatedLevelLoader>();
        }

        private static LevelLoader InstantiateConnectedLevelsLoader(Project.ConnectedLoadingStrategy strategy)
        {
            var obj = new GameObject();
            return strategy switch
            {
                Project.ConnectedLoadingStrategy.Neighbours => obj.AddComponent<NeighboursLevelLoader>(),
                Project.ConnectedLoadingStrategy.Worlds => obj.AddComponent<WorldsLevelLoader>(),
                Project.ConnectedLoadingStrategy.Areas => obj.AddComponent<AreasLevelLoader>(),
                _ => throw new System.InvalidOperationException("Unrecognized Loading Strategy: " + strategy)
            };
        }

        #endregion

        #region Fields

        protected Project _project;
        protected LevelBehaviour _currentBehaviour;
        protected LevelInfo _currentLevel;
        protected readonly Dictionary<string, LevelBehaviour> _registeredBehaviours = new();

        protected readonly Dictionary<string, GameObject> _loadedObjects = new();
        protected readonly Dictionary<string, SceneInstance> _loadedScenes = new();

        protected LevelActivationEvent _activationEventData = new();
        protected LevelDeactivationEvent _deactivationEventData = new();
        protected LevelPreparationEvent _preparationEventData = new();

        #endregion

        #region Getters

        /// <summary>
        /// The current project (<see cref="Project"/>).
        /// </summary>   
        public Project Project => _project;

        /// <summary>
        /// The navigation layer's name set in the LDtk project
        /// </summary>
        public string NavigationLayer => _project.NavigationLayer;

        /// <summary>
        /// The current level's behaviour (<see cref="LevelBehaviour"/>).
        /// </summary>
        public LevelBehaviour CurrentBehaviour => _currentBehaviour;

        #endregion 

        #region Initialization

        public void Initialize(Project project)
        {
            if (project == null) throw new System.ArgumentNullException(nameof(project));

            if (!ProjectsService.Instance.TryGetLdtkJson(project, out LdtkJson ldtkProject))
            {
                Logger.Error($"Failed to load LDtkJson for project {project.name}.", this);
                return;
            }

            _project = project;
            gameObject.name = "[LevelLoader] " + _project.name;

            LDtkIidBank.CacheIidData(ldtkProject);
        }

        #endregion       

        #region Behaviour

        protected virtual void Awake()
        {
        }

        #endregion

        #region Current Level

        protected virtual void DefineCurrentLevel(string iid)
        {
            // Tries to get a level by its Iid.
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                // If the level is not found, log the error and return false.
                Logger.Error($"Trying to set current level by Iid {iid} but it was not found.", this);
                return;
            }

            DefineCurrentLevel(level);

        }

        protected virtual void DefineCurrentLevel(LevelInfo level)
        {
            // Tries to get the MV_LevelBehaviour for the level.
            if (!_registeredBehaviours.TryGetValue(level.Iid, out LevelBehaviour behaviour))
            {
                // If the level is not registered, log the error and return false.
                Logger.Error($"Trying to define current level by Iid {level.Iid} but there is no behaviour for registered for it.", this);
                return;
            }

            _currentLevel = level;
            _currentBehaviour = behaviour;

            AfterLevelDefinition();
        }

        protected virtual void AfterLevelDefinition() { }

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
        protected virtual async UniTask LoadAsync(string iid)
        {
            // Check if the level exists
            if (!TryGetLevel(iid, out LevelInfo level)) return;
            await LoadAsync(level);
        }

        protected virtual async UniTask LoadAsync(LevelInfo level)
        {
            // Check if the level should be loaded as an object or a scene
            if (!level.WrappedInScene)
            {
                await LoadLevelObjectAsync(level);
            }
            else
            {
                await LoadLevelSceneAsync(level);
            }
        }

        protected virtual async UniTask LoadLevelObjectAsync(LevelInfo level)
        {
            // Check if the level has already been loaded
            if (_loadedObjects.ContainsKey(level.Iid)) return;

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
                _loadedObjects.Add(level.Iid, loadedObject);
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

        protected virtual async UniTask LoadLevelSceneAsync(LevelInfo level)
        {
            // Check if the level has already been loaded
            if (_loadedScenes.ContainsKey(level.Iid)) return;

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
                _loadedScenes.Add(level.Iid, sceneInstance);
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
            await UnloadAsync(level);
        }

        /// <summary>
        /// Unloads a level asynchronously. 
        /// </summary>
        /// <param name="level">The <see cref="LevelInfo"/> of the level to unload.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the level is unloaded.</returns>
        protected virtual async UniTask UnloadAsync(LevelInfo level)
        {
            // Check if the level has been loaded as an object
            if (_loadedObjects.TryGetValue(level.Iid, out GameObject loadedObject))
            {
                // Remove the level from the list of loaded objects
                _loadedObjects.Remove(level.Iid);

                // Destroy the loaded object
                Destroy(loadedObject);
                return;
            }

            // Check if the level has been loaded as a scene
            if (_loadedScenes.TryGetValue(level.Iid, out SceneInstance sceneInstance))
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
                _loadedScenes.Remove(level.Iid);
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

        #region Preparing Levels

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
            DefineCurrentLevel(iid);
            ConnectedLevelBehaviour connectedBehaviour = _currentBehaviour as ConnectedLevelBehaviour;

            // Prepare the level for entering through the main spot.
            if (!connectedBehaviour.Prepare(subject, out FlowSubjectTrail trail)) return;
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
            DefineCurrentLevel(iid);
            ConnectedLevelBehaviour connectedBehaviour = _currentBehaviour as ConnectedLevelBehaviour;
            // Prepare the level for entering through the spot.
            if (!connectedBehaviour.Prepare(subject, position, facingSign, out FlowSubjectTrail trail)) return;
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
            DefineCurrentLevel(iid);
            ConnectedLevelBehaviour connectedBehaviour = _currentBehaviour as ConnectedLevelBehaviour;

            // Prepare the level for entering through the spot.
            if (!connectedBehaviour.Prepare(subject, spotIid, out FlowSubjectTrail trail)) return;
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

        protected virtual void AnnounceActivation(LevelBehaviour behaviour)
        {
            _activationEventData.behaviour = behaviour;
            Bus<LevelActivationEvent>.Raise(_activationEventData);
        }

        protected virtual void AnnounceDeactivation(LevelBehaviour behaviour)
        {
            _activationEventData.behaviour = behaviour;
            Bus<LevelDeactivationEvent>.Raise(_deactivationEventData);
        }

        protected virtual void AnnouncePreparation(
            LevelBehaviour behaviour,
            ILevelFlowSubject subject,
            FlowSubjectTrail trail
        )
        {
            _preparationEventData.behaviour = behaviour;
            _preparationEventData.subject = subject;
            _preparationEventData.trail = trail;

            Bus<LevelPreparationEvent>.Raise(_preparationEventData);
        }

        #endregion

        #region Abstractions

        public abstract UniTask LoadLevel(string iid);
        public abstract UniTask LoadLevel(LevelInfo level);

        #endregion

        #region Casting

        public T As<T>() where T : LevelLoader
        {
            return this as T;
        }

        #endregion

    }
}