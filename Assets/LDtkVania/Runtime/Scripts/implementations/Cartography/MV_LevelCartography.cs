using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkVania.Cartography
{
    public class MV_LevelCartography
    {
        private string _name;
        private MV_Level _level;
        private MV_CartographyBounds _bounds;

        public MV_CartographyBounds Bounds => _bounds;

        public string Name => _name;
        public MV_Level Level => _level;

        public MV_LevelCartography(MV_Level level, int pixelsPerUnit, float scaleFactor = 1)
        {
            _name = level.Name;
            _level = level;
            Rect originalRect = level.LDtkLevel.UnityWorldSpaceBounds(WorldLayout.GridVania, pixelsPerUnit);
            _bounds = new MV_CartographyBounds(originalRect, scaleFactor);
        }
    }
}