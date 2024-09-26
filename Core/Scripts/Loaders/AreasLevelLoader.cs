using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace LDtkLevelManager
{
    [DefaultExecutionOrder(-1000)]
    public class AreasLevelLoader : UniverseLevelLoader
    {

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

            if (_currentLevel == null || _currentLevel.AreaName != level.AreaName)
            {
                await LoadArea(level.AreaName);
            }
        }

        /// <summary>
        /// Unloads all loaded levels and loads all levels of a given area (by name). If the area is not present in the project, <br/>
        /// an error will be logged and no action will be taken.<br/>
        /// <br/>
        /// Your LDtk project must have an area (area enum) with the given name.
        /// </summary>
        /// <param name="worldName">The name of the world to load.</param>
        /// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
        public virtual async UniTask LoadArea(string areaName)
        {
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
        protected override bool EvaluateAndPrepareLevel(string iid, out UniverseLevelBehaviour behaviour)
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

            _currentLevel = level;
            _currentBehaviour = behaviour;

            // If the level was prepared, return true.
            return true;
        }

        #endregion
    }
}