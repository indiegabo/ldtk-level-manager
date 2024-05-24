using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkVania.Cartography
{
    public class MV_AreaCartography
    {
        private string _areaName;
        private string _worldName;
        private MV_CartographyBounds _bounds;
        private Dictionary<string, MV_LevelCartography> _levels; // Key = Level Iid

        public string AreaName => _areaName;
        public string WorldName => _worldName;

        public MV_CartographyBounds Bounds => _bounds;

        public MV_AreaCartography(string areaName, string worldName, List<MV_LevelCartography> levels)
        {
            _areaName = areaName;
            _worldName = worldName;

            _levels = new Dictionary<string, MV_LevelCartography>();

            if (levels.Count == 0) return;

            _bounds = new MV_CartographyBounds(levels[0].Bounds);

            foreach (MV_LevelCartography levelCartography in levels)
            {
                AddLevel(levelCartography);
            }
        }

        public List<MV_LevelCartography> GetAllLevels()
        {
            return _levels.Values.ToList();
        }

        public MV_LevelCartography GetLevel(string levelIid)
        {
            if (!_levels.TryGetValue(levelIid, out MV_LevelCartography mvLevelCartography)) return null;
            return mvLevelCartography;
        }

        public bool TryGetLevel(string levelIid, out MV_LevelCartography mvLevelCartography)
        {
            return _levels.TryGetValue(levelIid, out mvLevelCartography);
        }

        private void AddLevel(MV_LevelCartography levelCartography)
        {
            _bounds.Expand(levelCartography.Bounds);
            _levels.Add(levelCartography.Level.Iid, levelCartography);
        }
    }
}