using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
using System.Linq;
using LDtkLevelManager.Cartography;

namespace LDtkLevelManager
{
    public partial class Project : ScriptableObject
    {

        #region Serializing

        [SerializeField] private LDtkProjectFile _ldtkProjectFile;
        [SerializeField] private bool _syncLevelsAtCompile = true;
        [SerializeField] private string _navigationLayer;
        [SerializeField] private ProjectCartography _cartography = new();
        [SerializeField] private InfoDictionary _levels = new();
        [SerializeField] private InfoDictionary _lostLevels = new();
        [SerializeField] private WorldInfoDictionary _worldInfoRegistry = new();

        #endregion

        #region Fields

        private LdtkJson _ldtkProject;

        #endregion

        #region Getters

        /// <summary>
        /// [Inspector only] <br/><br/>
        /// Whether levels should be synchronized when the editor is compiled.<br/>
        /// </summary>
        public bool SyncLevelsAtCompile => _syncLevelsAtCompile;

        /// <summary>
        /// The navigation layer defined in the LDtk project.<br/>
        /// </summary>
        public string NavigationLayer => _navigationLayer;

        /// <summary>
        /// The cartography settings for this project.
        /// </summary>
        public ProjectCartography Cartography => _cartography;

        /// <summary>
        /// Whether the LDtk project has been imported and the project is initialized.
        /// </summary>
        public bool IsInitialized => _ldtkProjectFile != null;

        /// <summary>
        /// The number of levels in the project.
        /// </summary>
        public int LevelsCount => _levels.Count;

        /// <summary>
        /// The LDtk project file.
        /// </summary>
        public LDtkProjectFile LDtkProjectFile => _ldtkProjectFile;

        /// <summary>
        /// The LDtk project JSON.
        /// </summary>
        public LdtkJson LDtkProject => _ldtkProjectFile.FromJson;

        /// <summary>
        /// The number of pixels per unit in the LDtk project.
        /// </summary>
        public int PixelsPerUnit => LDtkProject.DefaultGridSize;

        /// <summary>
        /// A dictionary of world names and their world info.
        /// </summary>
        public WorldInfoDictionary WorldAreas => _worldInfoRegistry;

        #endregion

        #region Providing Levels

        /// <summary>
        /// Retrieves a level by its Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to retrieve.</param>
        /// <returns>The retrieved level if successful, or null if not.</returns>
        public LevelInfo GetLevel(string iid)
        {
            if (!_levels.ContainsKey(iid)) return null;
            return _levels[iid];
        }

        /// <summary>
        /// Attempts to retrieve a level by its Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to retrieve.</param>
        /// <param name="level">The retrieved level if successful, or null if not.</param>
        /// <returns>true if the level was successfully retrieved, false otherwise.</returns>
        public bool TryGetLevel(string iid, out LevelInfo levelInfo)
        {
            return _levels.TryGetValue(iid, out levelInfo);
        }

        /// <summary>
        /// Checks if the project contains a level with the given Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to check.</param>
        /// <returns>true if the level is in the project, false otherwise.</returns>
        public bool HasLevel(string iid) => _levels.ContainsKey(iid);

        /// <summary>
        /// Retrieves all levels in the project.
        /// </summary>
        /// <returns>A list of all levels in the project.</returns>
        public List<LevelInfo> GetAllLevels()
        {
            return _levels.Values.ToList();
        }

        #endregion

        #region World and areas

        /// <summary>
        /// Retrieves all levels in a given world.
        /// </summary>
        /// <param name="worldName">The name of the world to retrieve levels from.</param>
        /// <returns>A list of all levels in the given world, or null if the world is not present in the project.</returns>
        public List<LevelInfo> GetAllLevelsInWorld(string worldName)
        {
            if (!_worldInfoRegistry.ContainsKey(worldName)) return null;

            List<LevelInfo> levels = new();
            foreach (LevelInfo level in _levels.Values)
            {
                if (level.WorldName == worldName) levels.Add(level);
            }
            return levels;
        }

        /// <summary>
        /// Retrieves all the Iids of the levels in a given world.
        /// </summary>
        /// <param name="worldName">The name of the world to retrieve levels from.</param>
        /// <returns>A set of all the Iids of the levels in the given world, or null if the world is not present in the project.</returns>
        public HashSet<string> GetAllLevelsIidsInWorld(string worldName)
        {
            if (!_worldInfoRegistry.ContainsKey(worldName)) return null;

            HashSet<string> iids = new();
            foreach (LevelInfo level in _levels.Values)
            {
                if (level.WorldName == worldName) iids.Add(level.Iid);
            }

            return iids;
        }

        /// <summary>
        /// Retrieves all the Iids of the levels in a given area.
        /// </summary>
        /// <param name="areaName">The name of the area to retrieve levels from.</param>
        /// <returns>A set of all the Iids of the levels in the given area, or an empty set if the area is not present in the project.</returns>
        public HashSet<string> GetAllLevelsIidsInArea(string areaName)
        {
            HashSet<string> iids = new();
            foreach (LevelInfo level in _levels.Values)
            {
                if (level.AreaName == areaName) iids.Add(level.Iid);
            }

            return iids;
        }

        /// <summary>
        /// Retrieves all world info objects in the project.
        /// </summary>
        /// <returns>A list of all world info objects in the project.</returns>
        public List<WorldInfo> GetAllWorldInfos()
        {
            return _worldInfoRegistry.Values.ToList();
        }


        /// <summary>
        /// Retrieves the world info for a given world name.
        /// </summary>
        /// <param name="worldName">The name of the world to retrieve info from.</param>
        /// <returns>The retrieved world info if successful, or null if the world is not present in the project.</returns>
        public WorldInfo GetWorldInfo(string worldName)
        {
            if (!_worldInfoRegistry.ContainsKey(worldName)) return null;
            return _worldInfoRegistry[worldName];
        }


        /// <summary>
        /// Tries to retrieve the world info for a given world name.
        /// </summary>
        /// <param name="worldName">The name of the world to retrieve info from.</param>
        /// <param name="worldInfo">The retrieved world info if successful, or null if not.</param>
        /// <returns>true if the world info was successfully retrieved, false otherwise.</returns>
        public bool TryGetWorldInfo(string worldName, out WorldInfo worldInfo)
        {
            return _worldInfoRegistry.TryGetValue(worldName, out worldInfo);
        }


        /// <summary>
        /// Tries to retrieve the world info for a given level.
        /// </summary>
        /// <param name="level">The level to retrieve info from.</param>
        /// <param name="worldInfo">The retrieved world info if successful, or null if not.</param>
        /// <returns>true if the world areas were successfully retrieved, false otherwise.</returns>
        /// <remarks>
        /// The world name of the level must be set for this method to work.
        /// </remarks>
        public bool TryGetWorldInfo(LevelInfo level, out WorldInfo worldInfo)
        {
            if (string.IsNullOrEmpty(level.WorldName))
            {
                Logger.Warning($"Trying to get world areas for level {level.Name}({level.Iid}) but it has no world name defined.", this);
                worldInfo = null;
                return false;
            }
            return _worldInfoRegistry.TryGetValue(level.WorldName, out worldInfo);
        }

        #endregion

    }
}