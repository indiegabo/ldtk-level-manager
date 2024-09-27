using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace LDtkLevelManager
{
    public abstract class ConnectedLevelLoader : LevelLoader
    {

        #region Fields

        protected readonly Dictionary<string, GameObject> _standAloneObjects = new();
        protected readonly Dictionary<string, SceneInstance> _standAloneScenes = new();

        protected readonly HashSet<string> _shouldBeLoaded = new();
        protected readonly HashSet<string> _shouldBeUnloaded = new();

        #endregion

        #region Getters        

        /// <summary>
        /// The current level (<see cref="LevelInfo"/>).
        /// </summary>
        public LevelInfo CurrentLevel => _currentLevel;


        public bool InStandAloneLevel => _currentLevel.StandAlone;

        #endregion

        #region Abstractions


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
            HashSet<string> toUnload = new();
            foreach (string iid in _loadedObjects.Keys)
            {
                toUnload.Add(iid);
            }

            foreach (string iid in _loadedScenes.Keys)
            {
                toUnload.Add(iid);
            }

            await UnloadMultipleAsync(toUnload);
        }

        #endregion

        #region Preparing level

        /// <summary>
        /// Prepares the level to be entered through a connection.<br/>       
        /// <br/>
        /// <b>Important:</b> This should be called when curtains are closed.<br/>
        /// <br/>
        /// </summary>
        /// <param name="connection">The connection to use to enter the level.</param>
        public virtual void Prepare(ILevelFlowSubject subject, string iid, IConnection connection)
        {
            DefineCurrentLevel(iid);
            ConnectedLevelBehaviour connectedBehaviour = _currentBehaviour as ConnectedLevelBehaviour;

            // Prepare the level for entering through the connection.
            if (!connectedBehaviour.Prepare(subject, connection, out FlowSubjectTrail trail)) return;
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
            DefineCurrentLevel(iid);
            ConnectedLevelBehaviour connectedBehaviour = _currentBehaviour as ConnectedLevelBehaviour;

            // Prepare the level for entering through the portal.
            if (!connectedBehaviour.Prepare(subject, portal, out FlowSubjectTrail trail)) return;

            AnnouncePreparation(_currentBehaviour, subject, trail);
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

            await LoadStandaloneAsync(level.Iid);
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

        protected virtual async UniTask LoadStandaloneAsync(string iid)
        {
            // Check if the level exists
            if (!TryGetLevel(iid, out LevelInfo level)) return;
            await LoadAsync(level);
        }

        protected virtual async UniTask LoadStandaloneAsync(LevelInfo level)
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

        protected virtual async UniTask LoadStandaloneObjectAsync(LevelInfo level)
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
                _standAloneObjects.Add(level.Iid, loadedObject);
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

        protected virtual async UniTask LoadStandaloneeSceneAsync(LevelInfo level)
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
                _standAloneScenes.Add(level.Iid, sceneInstance);
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