using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkVania.Cartography
{
    public class MV_WorldCartography
    {
        private string _worldName;
        private Rect _rect;
        private Rect _scaledRect;
        private Dictionary<string, MV_AreaCartography> _areas; // Key = Area name

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

        public MV_WorldCartography(string worldName, List<MV_AreaCartography> areas)
        {
            _worldName = worldName;
            _rect = new Rect();
            _scaledRect = new Rect();
            _areas = new Dictionary<string, MV_AreaCartography>();

            foreach (MV_AreaCartography area in areas)
            {
                AddArea(area);
            }
        }

        public List<MV_AreaCartography> GetAllAreas()
        {
            return _areas.Values.ToList();
        }

        public MV_AreaCartography GetArea(string areaName)
        {
            if (!_areas.TryGetValue(areaName, out MV_AreaCartography mvAreaCartography)) return null;
            return mvAreaCartography;
        }

        public bool TryGetArea(string areaName, out MV_AreaCartography mvAreaCartography)
        {
            return _areas.TryGetValue(areaName, out mvAreaCartography);
        }

        private void AddArea(MV_AreaCartography area)
        {
            float minX = Mathf.Min(_rect.min.x, area.Rect.min.x);
            float minY = Mathf.Min(_rect.min.y, area.Rect.min.y);
            float maxX = Mathf.Max(_rect.max.x, area.Rect.max.x);
            float maxY = Mathf.Max(_rect.max.y, area.Rect.max.y);
            _rect = new Rect(minX, minY, maxX - minX, maxY - minY);

            float scaledMinX = Mathf.Min(_scaledRect.min.x, area.ScaledRect.min.x);
            float scaledMinY = Mathf.Min(_scaledRect.min.y, area.ScaledRect.min.y);
            float scaledMaxX = Mathf.Max(_scaledRect.max.x, area.ScaledRect.max.x);
            float scaledMaxY = Mathf.Max(_scaledRect.max.y, area.ScaledRect.max.y);
            _scaledRect = new Rect(scaledMinX, scaledMinY, scaledMaxX - scaledMinX, scaledMaxY - scaledMinY);

            _areas.Add(area.AreaName, area);
        }
    }
}