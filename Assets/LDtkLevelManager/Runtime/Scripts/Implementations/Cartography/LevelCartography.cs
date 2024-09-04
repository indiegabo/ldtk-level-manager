using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager.Cartography
{
    public class LevelCartography
    {
        private string _name;
        private LevelInfo _info;
        private CartographyBounds _bounds;

        public CartographyBounds Bounds => _bounds;

        public string Name => _name;
        public LevelInfo Info => _info;

        public LevelCartography(LevelInfo info, int pixelsPerUnit, float scaleFactor = 1)
        {
            _name = info.Name;
            _info = info;
            Rect originalRect = info.LDtkLevel.UnityWorldSpaceBounds(WorldLayout.GridVania, pixelsPerUnit);
            _bounds = new CartographyBounds(originalRect, scaleFactor);
        }
    }
}