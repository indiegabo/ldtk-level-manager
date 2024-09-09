using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager.Cartography
{
    public class AreaCartography
    {
        private string _areaName;
        private string _worldName;
        private CartographyBounds _bounds;
        private Dictionary<string, LevelCartography> _levels; // Key = Level Iid

        /// <summary>
        /// The name of the area.
        /// </summary>
        public string AreaName => _areaName;

        /// <summary>
        /// The name of the world.
        /// </summary>
        public string WorldName => _worldName;

        /// <summary>
        /// The bounds of the area in the world.
        /// </summary>
        public CartographyBounds Bounds => _bounds;

        public AreaCartography(string areaName, string worldName, List<LevelCartography> levels)
        {
            _areaName = areaName;
            _worldName = worldName;

            _levels = new Dictionary<string, LevelCartography>();

            if (levels.Count == 0) return;

            _bounds = new CartographyBounds(levels[0].Bounds);

            foreach (LevelCartography levelCartography in levels)
            {
                AddLevel(levelCartography);
            }
        }

        /// <summary>
        /// Retrieves all the levels in the area.
        /// </summary>
        /// <returns>A list of all the levels in the area.</returns>
        public List<LevelCartography> GetAllLevels()
        {
            return _levels.Values.ToList();
        }

        /// <summary>
        /// Retrieves a level from the area given its Iid.
        /// </summary>
        /// <param name="levelIid">The Iid of the level to retrieve.</param>
        /// <returns>The retrieved level if successful, or null if the level is not in the area.</returns>
        public LevelCartography GetLevel(string levelIid)
        {
            if (!_levels.TryGetValue(levelIid, out LevelCartography levelInfoCartography)) return null;
            return levelInfoCartography;
        }

        /// <summary>
        /// Tries to retrieve a level from the area given its Iid.
        /// </summary>
        /// <param name="levelIid">The Iid of the level to retrieve.</param>
        /// <param name="levelCartography">The retrieved level if successful, or null if the level is not in the area.</param>
        /// <returns>true if the level was successfully retrieved, false otherwise.</returns>
        public bool TryGetLevel(string levelIid, out LevelCartography levelCartography)
        {
            return _levels.TryGetValue(levelIid, out levelCartography);
        }

        private void AddLevel(LevelCartography levelCartography)
        {
            _bounds.Expand(levelCartography.Bounds);
            _levels.Add(levelCartography.Info.Iid, levelCartography);
        }
    }
}