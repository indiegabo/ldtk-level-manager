using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LDtkVania.Cartography
{
    public class MV_Cartographer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private MV_Project _project;

        [SerializeField]
        private int _pixelsPerUnit = 16;

        [SerializeField]
        private float _scaleFactor = 1;

        #endregion

        #region Fields

        private Dictionary<string, MV_LevelCartography> _levels;
        private Dictionary<string, MV_WorldCartography> _worlds;

        #endregion

        #region Behaviour

        private void Awake()
        {
            GenerateCartography();
            // LogWorlds();
        }

        #endregion

        #region Generating

        /// <summary>
        /// Generate the cartography for all levels and areas of the project.
        /// </summary>
        private void GenerateCartography()
        {
            _levels = new Dictionary<string, MV_LevelCartography>();
            _worlds = new Dictionary<string, MV_WorldCartography>();
            List<MV_Level> levels = _project.GetAllLevels();

            // Build a dictionary with a key that combines the world and area names,
            // and the value is a list of level cartographies that belong to that world and area.
            Dictionary<string, List<MV_LevelCartography>> levelsByWorldAndArea = new();

            foreach (MV_Level level in levels)
            {
                if (string.IsNullOrEmpty(level.WorldName) || string.IsNullOrEmpty(level.AreaName)) continue;

                // Create a key that combines the world and area names.
                string key = level.WorldName + "_" + level.AreaName;

                // If the key doesn't already exist, add it to the dictionary.
                if (!levelsByWorldAndArea.ContainsKey(key))
                {
                    levelsByWorldAndArea.Add(key, new List<MV_LevelCartography>());
                }

                // Add the level cartography to the list for the key.
                List<MV_LevelCartography> levelsList = levelsByWorldAndArea[key];
                MV_LevelCartography levelCartography = new(level, _pixelsPerUnit, _scaleFactor);
                levelsList.Add(levelCartography);
                _levels.Add(level.Iid, levelCartography);
            }

            // Get all the world areas from the project.
            List<MV_WorldAreas> worldAreas = _project.GetAllWorldAreas();

            foreach (MV_WorldAreas worldArea in worldAreas)
            {
                if (worldArea.areas.Count == 0) continue;

                // Create a dictionary with a key that is the area name,
                // and the value is the area cartography.
                Dictionary<string, MV_AreaCartography> cartographyAreas = new();

                foreach (string area in worldArea.areas)
                {
                    if (string.IsNullOrEmpty(area)) continue;

                    // Create a key that combines the world and area names
                    // So that we can get the list of level cartographies for the world and area.
                    string key = worldArea.worldName + "_" + area;

                    // Get the list of level cartographies for the world and area.
                    List<MV_LevelCartography> levelsList = levelsByWorldAndArea[key];

                    // Create the area cartography and add it to the dictionary.
                    MV_AreaCartography areaCartography = new(area, worldArea.worldName, levelsList);
                    cartographyAreas.Add(key, areaCartography);
                }

                // Create a world cartography object with its area cartographies and add it to the list.
                MV_WorldCartography worldCartography = new(worldArea.worldName, cartographyAreas.Values.ToList());
                _worlds.Add(worldArea.worldName, worldCartography);
            }

            LogWorlds();
        }

        #endregion

        #region Providers

        public List<MV_WorldCartography> GetAllWorlds()
        {
            return _worlds.Values.ToList();
        }

        public MV_WorldCartography GetWorld(string worldName)
        {
            if (!_worlds.TryGetValue(worldName, out MV_WorldCartography worldCartography)) return null;
            return worldCartography;
        }

        public bool TryGetWorld(string worldName, out MV_WorldCartography worldCartography)
        {
            return _worlds.TryGetValue(worldName, out worldCartography);
        }

        public MV_AreaCartography GetArea(string worldName, string areaName)
        {
            if (!_worlds.TryGetValue(worldName, out MV_WorldCartography worldCartography)) return null;
            return worldCartography.GetArea(areaName);
        }

        public bool TryGetArea(string worldName, string areaName, out MV_AreaCartography areaCartography)
        {
            if (!_worlds.TryGetValue(worldName, out MV_WorldCartography worldCartography))
            {
                areaCartography = null;
                return false;
            }
            return worldCartography.TryGetArea(areaName, out areaCartography);
        }

        public MV_LevelCartography GetLevel(string levelIid)
        {
            if (!_levels.TryGetValue(levelIid, out MV_LevelCartography mvLevelCartography)) return null;
            return mvLevelCartography;
        }

        public bool TryGetLevel(string levelIid, out MV_LevelCartography levelCartography)
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
            foreach (MV_WorldCartography world in _worlds.Values)
            {
                Debug.Log($"===== World: {world.WorldName} =====");
                Debug.Log($"Size: {world.Bounds.Size}");
                Debug.Log($"Scaled Size: {world.Bounds.ScaledSize}");
                foreach (MV_AreaCartography area in world.GetAllAreas())
                {
                    Debug.Log($"=== Area: {area.AreaName} ===");
                    Debug.Log($"Size: {area.Bounds.Size}");
                    Debug.Log($"Scaled Size: {area.Bounds.ScaledSize}");
                    foreach (MV_LevelCartography level in area.GetAllLevels())
                    {
                        Debug.Log($"- Level: {level.Level.Name} -");
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