using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager.Cartography
{

    public class Cartographer : MonoBehaviour
    {
        #region Static

        private static Dictionary<string, Cartographer> _cartographers;

        public static void RegisterCartographers(List<CartographerEntry> entries)
        {
            _cartographers = new();
            foreach (CartographerEntry entry in entries)
            {
                _cartographers.Add(entry.project.Iid, entry.cartographer);
            }
        }

        public class CartographerEntry { public Project project; public Cartographer cartographer; }

        public static Cartographer For(Project project)
        {
            if (!_cartographers.TryGetValue(project.Iid, out Cartographer cartographer))
            {
                Logger.Error($"Cartographer for project {project.name} not found.", project);
                return null;
            }

            return cartographer;
        }

        #endregion

        #region Inspector


        #endregion

        #region Fields

        private Project _project;
        private LdtkJson _ldtkJson;

        private Dictionary<string, LevelCartography> _levels;
        private Dictionary<string, WorldCartography> _worlds;

        #endregion

        #region Getters

        public int PixelsPerUnit => _ldtkJson.DefaultGridSize;
        public float ScaleFactor => _project.Cartography.scaleFactor;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the cartographer with the given project and LDtkJson.<br/>
        /// <b>IMPORTANT:</b> This is automatically called by Unity when runtime starts. You do not
        /// need to instantiate cartographers yourself.
        /// </summary>
        /// <param name="project">The project to initialize the cartographer with.</param>
        /// <param name="ldtkJson">The LDtkJson of the project to initialize the cartographer with.</param>
        public void Initialize(Project project, LdtkJson ldtkJson)
        {
            _project = project;
            _ldtkJson = ldtkJson;

            GenerateCartography(_project);
        }

        #endregion

        #region Generating

        /// <summary>
        /// Generate the cartography for all levels and areas of the project.
        /// </summary>
        private void GenerateCartography(Project project)
        {
            _levels = new Dictionary<string, LevelCartography>();
            _worlds = new Dictionary<string, WorldCartography>();
            List<LevelInfo> levels = project.GetAllLevels();

            // Build a dictionary with a key that combines the world and area names,
            // and the value is a list of level cartographies that belong to that world and area.
            Dictionary<string, List<LevelCartography>> levelsByWorldAndArea = new();

            foreach (LevelInfo level in levels)
            {
                if (string.IsNullOrEmpty(level.WorldName) || string.IsNullOrEmpty(level.AreaName)) continue;

                // Create a key that combines the world and area names.
                string key = level.WorldName + "_" + level.AreaName;

                // If the key doesn't already exist, add it to the dictionary.
                if (!levelsByWorldAndArea.ContainsKey(key))
                {
                    levelsByWorldAndArea.Add(key, new List<LevelCartography>());
                }

                // Add the level cartography to the list for the key.
                List<LevelCartography> levelsList = levelsByWorldAndArea[key];
                LevelCartography levelCartography = new(level, PixelsPerUnit, ScaleFactor);
                levelsList.Add(levelCartography);
                _levels.Add(level.Iid, levelCartography);
            }

            // Get all the world areas from the project.
            List<WorldInfo> infos = _project.GetAllWorldInfos();

            foreach (WorldInfo info in infos)
            {
                if (info.areas.Count == 0) continue;

                // Create a dictionary with a key that is the area name,
                // and the value is the area cartography.
                Dictionary<string, AreaCartography> cartographyAreas = new();

                foreach (string area in info.areas)
                {
                    if (string.IsNullOrEmpty(area)) continue;

                    // Create a key that combines the world and area names
                    // So that we can get the list of level cartographies for the world and area.
                    string key = info.name + "_" + area;

                    // Get the list of level cartographies for the world and area.
                    List<LevelCartography> levelsList = levelsByWorldAndArea[key];

                    // Create the area cartography and add it to the dictionary.
                    AreaCartography areaCartography = new(area, info.name, levelsList);
                    cartographyAreas.Add(key, areaCartography);
                }

                // Create a world cartography object with its area cartographies and add it to the list.
                WorldCartography worldCartography = new(info.name, cartographyAreas.Values.ToList());
                _worlds.Add(info.name, worldCartography);
            }
        }

        #endregion

        #region Providers

        /// <summary>
        /// Retrieves all the world cartographies from the project.
        /// </summary>
        /// <returns>A list of all world cartographies in the project.</returns>
        public List<WorldCartography> GetAllWorlds()
        {
            return _worlds.Values.ToList();
        }

        /// <summary>
        /// Retrieves the cartography of a given world from the project.
        /// </summary>
        /// <param name="worldName">The name of the world to retrieve.</param>
        /// <returns>The retrieved cartography if successful, or null if the world is not present in the project.</returns>
        public WorldCartography GetWorldCartography(string worldName)
        {
            if (!_worlds.TryGetValue(worldName, out WorldCartography worldCartography)) return null;
            return worldCartography;
        }

        /// <summary>
        /// Tries to retrieve the cartography of a given world from the project.
        /// </summary>
        /// <param name="worldName">The name of the world to retrieve.</param>
        /// <param name="worldCartography">The retrieved cartography if successful, or null if not.</param>
        /// <returns>true if the cartography was successfully retrieved, false otherwise.</returns>
        public bool TryWorldCartography(string worldName, out WorldCartography worldCartography)
        {
            return _worlds.TryGetValue(worldName, out worldCartography);
        }


        public bool TryWorldCartography(World world, out WorldCartography worldCartography)
        {
            return TryWorldCartography(world.Identifier, out worldCartography);
        }

        /// <summary>
        /// Retrieves the cartography of a given area from the project.
        /// </summary>
        /// <param name="worldName">The name of the world where the area is located.</param>
        /// <param name="areaName">The name of the area to retrieve.</param>
        /// <returns>The retrieved cartography if successful, or null if the world or area is not present in the project.</returns>
        public AreaCartography GetAreaCartography(string worldName, string areaName)
        {
            if (!_worlds.TryGetValue(worldName, out WorldCartography worldCartography)) return null;
            return worldCartography.GetArea(areaName);
        }

        /// <summary>
        /// Tries to retrieve the cartography of a given area from the project.
        /// </summary>
        /// <param name="worldName">The name of the world where the area is located.</param>
        /// <param name="areaName">The name of the area to retrieve.</param>
        /// <param name="areaCartography">The retrieved cartography if successful, or null if not.</param>
        /// <returns>true if the cartography was successfully retrieved, false otherwise.</returns>
        public bool TryAreaCartography(string worldName, string areaName, out AreaCartography areaCartography)
        {
            if (!_worlds.TryGetValue(worldName, out WorldCartography worldCartography))
            {
                areaCartography = null;
                return false;
            }
            return worldCartography.TryGetArea(areaName, out areaCartography);
        }

        /// <summary>
        /// Retrieves the cartography of a level given its Iid.
        /// </summary>
        /// <param name="levelIid">The Iid of the level to retrieve.</param>
        /// <returns>The retrieved cartography if successful, or null if not.</returns>
        public LevelCartography GetLevelCartography(string levelIid)
        {
            if (!_levels.TryGetValue(levelIid, out LevelCartography levelInfoCartography)) return null;
            return levelInfoCartography;
        }

        /// <summary>
        /// Retrieves the cartography of a level given its <see cref="LevelInfo"/>.
        /// </summary>
        /// <param name="level">The <see cref="LevelInfo"/> of the level to retrieve.</param>
        /// <returns>The retrieved cartography if successful, or null if not.</returns>
        public LevelCartography GetLevelCartography(LevelInfo level)
        {
            return GetLevelCartography(level.Iid);
        }

        /// <summary>
        /// Tries to retrieve the cartography of a level given its Iid.
        /// </summary>
        /// <param name="levelIid">The Iid of the level to retrieve.</param>
        /// <param name="levelCartography">The retrieved cartography if successful, or null if not.</param>
        /// <returns>true if the cartography was successfully retrieved, false otherwise.</returns>
        public bool TryLevelCartography(string levelIid, out LevelCartography levelCartography)
        {
            return _levels.TryGetValue(levelIid, out levelCartography);
        }

        /// <summary>
        /// Tries to retrieve the cartography of a level given its LevelInfo.
        /// </summary>
        /// <param name="level">The LevelInfo of the level to retrieve.</param>
        /// <param name="levelCartography">The retrieved cartography if successful, or null if not.</param>
        /// <returns>true if the cartography was successfully retrieved, false otherwise.</returns>
        public bool TryLevelCartography(LevelInfo level, out LevelCartography levelCartography)
        {
            return TryLevelCartography(level.Iid, out levelCartography);
        }

        /// <summary>
        /// Calculates a position in world coordinates scaled by the ScaleFactor 
        /// value of the Project.
        /// </summary>
        /// <param name="positionInWorld">The position in world coordinates.</param>
        /// <returns>The scaled position in world coordinates.</returns>
        public Vector2 CalculateScaledPosition(Vector2 positionInWorld)
        {
            return positionInWorld * ScaleFactor;
        }

        #endregion

        #region Debugging

        public void LogWorlds()
        {
            foreach (WorldCartography world in _worlds.Values)
            {
                Debug.Log($"===== World: {world.WorldName} =====");
                Debug.Log($"Size: {world.Bounds.Size}");
                Debug.Log($"Scaled Size: {world.Bounds.ScaledSize}");
                foreach (AreaCartography area in world.GetAllAreas())
                {
                    Debug.Log($"=== Area: {area.AreaName} ===");
                    Debug.Log($"Size: {area.Bounds.Size}");
                    Debug.Log($"Scaled Size: {area.Bounds.ScaledSize}");
                    foreach (LevelCartography level in area.GetAllLevels())
                    {
                        Debug.Log($"- Level: {level.Info.Name} -");
                        Debug.Log($"Size: {level.Bounds.Size} - Scaled Size: {level.Bounds.ScaledSize}");
                        Debug.Log($"Min: {level.Bounds.Min} - Max: {level.Bounds.Max}");
                        Debug.Log($"Scaled Min: {level.Bounds.ScaledMin} - Max: {level.Bounds.ScaledMax}");
                    }
                }
            }
        }

        #endregion
    }
}