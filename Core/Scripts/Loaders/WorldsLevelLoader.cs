using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Linq;
using LDtkUnity;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.ResourceManagement.Exceptions;
using LDtkLevelManager.EventBus;

namespace LDtkLevelManager
{
    [DefaultExecutionOrder(-1000)]
    public class WorldsLevelLoader : ConnectedLevelLoader
    {

        #region Behaviour

        protected override void Awake()
        {
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
        public override async UniTask LoadLevel(string iid)
        {
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                Logger.Error($"Level under LDtk Iid {iid} not present in project {_project.name}", this);
                return;
            }

            await LoadLevel(level);
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
        public override async UniTask LoadLevel(LevelInfo level)
        {
            if (level.StandAlone)
            {
                Logger.Error($"Level {level.Iid} is standalone and cannot be loaded as a Universe level.", this);
                return;
            }

            if (_currentLevel == null || _currentLevel.WorldName != level.WorldName)
            {
                await LoadWorld(level.WorldName);
            }
        }

        #endregion


        /// <summary>
        /// Unloads all loaded levels and loads all levels of a given world (by name). If the world is not present in the project, <br/>
        /// an error will be logged and no action will be taken.<br/>
        /// <br/>
        /// Your LDtk project must have a world with the given name.
        /// </summary>
        /// <param name="worldName">The name of the world to load.</param>
        /// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
        protected virtual async UniTask LoadWorld(string worldName)
        {
            // Get all the Iids of the levels in the given world.
            HashSet<string> iids = _project.GetAllLevelsIidsInWorld(worldName);

            if (iids == null || iids.Count == 0)
            {
                Logger.Error($"Trying to load world {worldName} but it has no levels.", this);
                return;
            }


            /// Exit the current level before loading new ones.
            DeactivatePreparedLevel();


            /// Unload all loaded levels and objects before loading new ones.
            await UnloadEverything();

            /// Clear the lists of registered behaviors and loaded objects and scenes
            /// before loading new levels.
            _registeredBehaviours.Clear();
            _loadedObjects.Clear();
            _loadedScenes.Clear();

            /// Load all the levels in the given world.
            await LoadMultipleAsync(iids);
        }

    }
}