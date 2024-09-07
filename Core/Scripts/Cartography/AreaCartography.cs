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

        public string AreaName => _areaName;
        public string WorldName => _worldName;

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

        public List<LevelCartography> GetAllLevels()
        {
            return _levels.Values.ToList();
        }

        public LevelCartography GetLevel(string levelIid)
        {
            if (!_levels.TryGetValue(levelIid, out LevelCartography levelInfoCartography)) return null;
            return levelInfoCartography;
        }

        public bool TryGetLevel(string levelIid, out LevelCartography levelInfoCartography)
        {
            return _levels.TryGetValue(levelIid, out levelInfoCartography);
        }

        private void AddLevel(LevelCartography levelCartography)
        {
            _bounds.Expand(levelCartography.Bounds);
            _levels.Add(levelCartography.Info.Iid, levelCartography);
        }
    }
}