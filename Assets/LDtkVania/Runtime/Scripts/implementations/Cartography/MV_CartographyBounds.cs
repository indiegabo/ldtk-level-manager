
using UnityEngine;

namespace LDtkVania.Cartography
{
    public struct MV_CartographyBounds
    {
        private Rect _rect;
        private Rect _scaledRect;

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

        public MV_CartographyBounds(MV_CartographyBounds original)
        {
            _rect = new Rect()
            {
                x = original.Rect.x,
                y = original.Rect.y,
                width = original.Rect.width,
                height = original.Rect.height
            };
            _scaledRect = new Rect()
            {
                x = original.ScaledRect.x,
                y = original.ScaledRect.y,
                width = original.ScaledRect.width,
                height = original.ScaledRect.height
            };
        }

        public MV_CartographyBounds(Rect original, float scaleFactor)
        {
            _rect = original;
            _scaledRect = new Rect()
            {
                x = _rect.x * scaleFactor,
                y = _rect.y * scaleFactor,
                width = _rect.width * scaleFactor,
                height = _rect.height * scaleFactor
            };
        }

        public void Expand(MV_CartographyBounds containedBounds)
        {
            _rect.xMin = Mathf.Min(_rect.min.x, containedBounds.Rect.min.x);
            _rect.yMin = Mathf.Min(_rect.min.y, containedBounds.Rect.min.y);
            _rect.xMax = Mathf.Max(_rect.max.x, containedBounds.Rect.max.x);
            _rect.yMax = Mathf.Max(_rect.max.y, containedBounds.Rect.max.y);

            _scaledRect.xMin = Mathf.Min(_scaledRect.min.x, containedBounds.ScaledRect.min.x);
            _scaledRect.yMin = Mathf.Min(_scaledRect.min.y, containedBounds.ScaledRect.min.y);
            _scaledRect.xMax = Mathf.Max(_scaledRect.max.x, containedBounds.ScaledRect.max.x);
            _scaledRect.yMax = Mathf.Max(_scaledRect.max.y, containedBounds.ScaledRect.max.y);
        }
    }
}