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
        private Rect _rect;
        private Rect _scaledRect;

        public string Name => _name;
        public MV_Level Level => _level;
        public Rect LevelRect => _rect;
        public Vector2 Size => _rect.size;
        public Vector2 Center => _rect.center;
        public Vector2 Min => _rect.min;
        public Vector2 Max => _rect.max;

        public Rect ScaledRect => _scaledRect;
        public Vector2 ScaledSize => _scaledRect.size;
        public Vector2 ScaledCenter => _scaledRect.center;
        public Vector2 ScaledMin => _scaledRect.min;
        public Vector2 ScaledMax => _scaledRect.max;

        public MV_LevelCartography(MV_Level level, int pixelsPerUnit, float scaleFactor = 1)
        {
            _name = level.Name;
            _level = level;
            _rect = level.LDtkLevel.UnityWorldSpaceBounds(WorldLayout.GridVania, pixelsPerUnit);
            _scaledRect = new Rect()
            {
                x = _rect.x * scaleFactor,
                y = _rect.y * scaleFactor,
                width = _rect.width * scaleFactor,
                height = _rect.height * scaleFactor
            };
        }
    }
}