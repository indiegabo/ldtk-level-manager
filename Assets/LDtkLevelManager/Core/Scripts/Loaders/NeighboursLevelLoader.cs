using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
using Cysharp.Threading.Tasks;

namespace LDtkLevelManager
{
    [DefaultExecutionOrder(-1000)]
    public class NeighboursLevelLoader : ConnectedLevelLoader
    {

        #region Inspector        

        [SerializeField]
        [Min(1)]
        private int _depth = 1;

        #endregion

        #region Fields

        protected readonly Queue<(LevelInfo, int)> _neighboursQueue = new();

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

            await LoadNeighboursAsync(level);
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
        protected override bool EvaluateAndPrepareLevel(string iid, out ConnectedLevelBehaviour behaviour)
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


            if (!level.StandAlone)
            {
                _ = LoadNeighboursAsync(_currentLevel);
            }

            // If the level was prepared, return true.
            return true;
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
    }
}