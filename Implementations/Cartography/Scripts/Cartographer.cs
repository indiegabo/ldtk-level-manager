using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LDtkLevelManager.Cartography
{
    public class Cartographer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private Project _project;

        [SerializeField]
        private int _pixelsPerUnit = 16;

        [SerializeField]
        private float _scaleFactor = 1;

        #endregion

        #region Fields

        private Dictionary<string, LevelCartography> _levels;
        private Dictionary<string, WorldCartography> _worlds;

        #endregion

        #region Getters

        public int PixelsPerUnit => _pixelsPerUnit;
        public float ScaleFactor => _scaleFactor;

        #endregion

        #region Behaviour

        private void Awake()
        {
            GenerateCartography();
        }

        #endregion

        #region Generating

        /// <summary>
        /// Generate the cartography for all levels and areas of the project.
        /// </summary>
        private void GenerateCartography()
        {
            _levels = new Dictionary<string, LevelCartography>();
            _worlds = new Dictionary<string, WorldCartography>();
            List<LevelInfo> levels = _project.GetAllLevels();

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
                LevelCartography levelCartography = new(level, _pixelsPerUnit, _scaleFactor);
                levelsList.Add(levelCartography);
                _levels.Add(level.Iid, levelCartography);
            }

            // Get all the world areas from the project.
            List<WorldInfo> worldAreas = _project.GetAllWorldAreas();

            foreach (WorldInfo worldArea in worldAreas)
            {
                if (worldArea.areas.Count == 0) continue;

                // Create a dictionary with a key that is the area name,
                // and the value is the area cartography.
                Dictionary<string, AreaCartography> cartographyAreas = new();

                foreach (string area in worldArea.areas)
                {
                    if (string.IsNullOrEmpty(area)) continue;

                    // Create a key that combines the world and area names
                    // So that we can get the list of level cartographies for the world and area.
                    string key = worldArea.worldName + "_" + area;

                    // Get the list of level cartographies for the world and area.
                    List<LevelCartography> levelsList = levelsByWorldAndArea[key];

                    // Create the area cartography and add it to the dictionary.
                    AreaCartography areaCartography = new(area, worldArea.worldName, levelsList);
                    cartographyAreas.Add(key, areaCartography);
                }

                // Create a world cartography object with its area cartographies and add it to the list.
                WorldCartography worldCartography = new(worldArea.worldName, cartographyAreas.Values.ToList());
                _worlds.Add(worldArea.worldName, worldCartography);
            }
        }

        #endregion

        #region Providers

        public List<WorldCartography> GetAllWorlds()
        {
            return _worlds.Values.ToList();
        }

        public WorldCartography GetWorld(string worldName)
        {
            if (!_worlds.TryGetValue(worldName, out WorldCartography worldCartography)) return null;
            return worldCartography;
        }

        public bool TryGetWorld(string worldName, out WorldCartography worldCartography)
        {
            return _worlds.TryGetValue(worldName, out worldCartography);
        }

        public AreaCartography GetArea(string worldName, string areaName)
        {
            if (!_worlds.TryGetValue(worldName, out WorldCartography worldCartography)) return null;
            return worldCartography.GetArea(areaName);
        }

        public bool TryGetArea(string worldName, string areaName, out AreaCartography areaCartography)
        {
            if (!_worlds.TryGetValue(worldName, out WorldCartography worldCartography))
            {
                areaCartography = null;
                return false;
            }
            return worldCartography.TryGetArea(areaName, out areaCartography);
        }

        public LevelCartography GetLevel(string levelIid)
        {
            if (!_levels.TryGetValue(levelIid, out LevelCartography levelInfoCartography)) return null;
            return levelInfoCartography;
        }

        public bool TryGetLevel(string levelIid, out LevelCartography levelCartography)
        {
            return _levels.TryGetValue(levelIid, out levelCartography);
        }

        public Vector2 CalculateScaledPosition(Vector2 positionInWorld)
        {
            return positionInWorld * _scaleFactor;
        }

        #endregion

        #region Debugging

        private void LogWorlds()
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