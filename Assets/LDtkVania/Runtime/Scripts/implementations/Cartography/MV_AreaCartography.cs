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
        private Rect _rect;
        private Rect _scaledRect;
        private Dictionary<string, MV_LevelCartography> _levels; // Key = Level Iid

        public string AreaName => _areaName;
        public string WorldName => _worldName;

        public Rect Rect => _rect;
        public Vector2 Size => _rect.size;
        public Vector2 Center => _rect.center;
        public Vector2 Min => _rect.min;
        public Vector2 Max => _rect.max;

        public Rect ScaledRect => _scaledRect;
        public Vector2 ScaledSize => _scaledRect.size;
        public Vector2 ScaledCenter => _scaledRect.center;
        public Vector2 ScaledMin => _scaledRect.min;
        public Vector2 ScaledMax => _scaledRect.max;

        public MV_AreaCartography(string areaName, string worldName, List<MV_LevelCartography> levels)
        {
            _areaName = areaName;
            _worldName = worldName;
            _rect = new Rect();
            _scaledRect = new Rect();
            _levels = new Dictionary<string, MV_LevelCartography>();
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
            _levels.Add(levelCartography.Level.Iid, levelCartography);

            float minX = Mathf.Min(_rect.min.x, levelCartography.ScaledRect.min.x);
            float minY = Mathf.Min(_rect.min.y, levelCartography.ScaledRect.min.y);
            float maxX = Mathf.Max(_rect.max.x, levelCartography.ScaledRect.max.x);
            float maxY = Mathf.Max(_rect.max.y, levelCartography.ScaledRect.max.y);
            _rect = new Rect(minX, minY, maxX - minX, maxY - minY);

            float scaledMinX = Mathf.Min(_scaledRect.min.x, levelCartography.ScaledRect.min.x);
            float scaledMinY = Mathf.Min(_scaledRect.min.y, levelCartography.ScaledRect.min.y);
            float scaledMaxX = Mathf.Max(_scaledRect.max.x, levelCartography.ScaledRect.max.x);
            float scaledMaxY = Mathf.Max(_scaledRect.max.y, levelCartography.ScaledRect.max.y);
            _scaledRect = new Rect(scaledMinX, scaledMinY, scaledMaxX - scaledMinX, scaledMaxY - scaledMinY);
        }
    }
}