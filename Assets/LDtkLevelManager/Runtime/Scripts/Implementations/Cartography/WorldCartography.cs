using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager.Cartography
{
    public class WorldCartography
    {
        private string _worldName;
        private CartographyBounds _bounds;
        private Dictionary<string, AreaCartography> _areas; // Key = Area name

        public string WorldName => _worldName;
        public CartographyBounds Bounds => _bounds;

        public WorldCartography(string worldName, List<AreaCartography> areas)
        {
            _worldName = worldName;
            _areas = new Dictionary<string, AreaCartography>();

            if (areas.Count == 0) return;

            _bounds = new CartographyBounds(areas[0].Bounds);
            foreach (AreaCartography area in areas)
            {
                AddArea(area);
            }
        }

        public List<AreaCartography> GetAllAreas()
        {
            return _areas.Values.ToList();
        }

        public AreaCartography GetArea(string areaName)
        {
            if (!_areas.TryGetValue(areaName, out AreaCartography mvAreaCartography)) return null;
            return mvAreaCartography;
        }

        public bool TryGetArea(string areaName, out AreaCartography mvAreaCartography)
        {
            return _areas.TryGetValue(areaName, out mvAreaCartography);
        }

        private void AddArea(AreaCartography area)
        {
            _bounds.Expand(area.Bounds);
            _areas.Add(area.AreaName, area);
        }
    }
}