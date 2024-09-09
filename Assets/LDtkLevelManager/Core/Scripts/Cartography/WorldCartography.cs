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

        /// <summary>
        /// The name of the world.
        /// </summary>
        public string WorldName => _worldName;

        /// <summary>
        /// The bounds of the world.
        /// </summary>
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

        /// <summary>
        /// Gets a list of all area cartographies in this world.
        /// </summary>
        /// <returns>A list of area cartographies.</returns>
        public List<AreaCartography> GetAllAreas()
        {
            return _areas.Values.ToList();
        }

        /// <summary>
        /// Retrieves the cartography of a given area from the world.
        /// </summary>
        /// <param name="areaName">The name of the area to retrieve.</param>
        /// <returns>The retrieved cartography if successful, or null if the world or area is not present in the project.</returns>
        public AreaCartography GetArea(string areaName)
        {
            if (!_areas.TryGetValue(areaName, out AreaCartography mvAreaCartography)) return null;
            return mvAreaCartography;
        }

        /// <summary>
        /// Tries to retrieve the cartography of a given area from the world.
        /// </summary>
        /// <param name="areaName">The name of the area to retrieve.</param>
        /// <param name="mvAreaCartography">The retrieved cartography if successful, or null if not.</param>
        /// <returns>true if the cartography was successfully retrieved, false otherwise.</returns>
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