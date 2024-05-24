using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkVania.Cartography
{
    public class MV_WorldCartography
    {
        private string _worldName;
        private MV_CartographyBounds _bounds;
        private Dictionary<string, MV_AreaCartography> _areas; // Key = Area name

        public string WorldName => _worldName;
        public MV_CartographyBounds Bounds => _bounds;

        public MV_WorldCartography(string worldName, List<MV_AreaCartography> areas)
        {
            _worldName = worldName;
            _areas = new Dictionary<string, MV_AreaCartography>();

            if (areas.Count == 0) return;

            _bounds = new MV_CartographyBounds(areas[0].Bounds);
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
            _bounds.Expand(area.Bounds);
            _areas.Add(area.AreaName, area);
        }
    }
}