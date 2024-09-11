
using UnityEngine;

namespace LDtkLevelManager.Cartography
{
    public struct CartographyBounds
    {
        public static Rect Scale(Rect rect, float scaleFactor)
        {
            return new Rect(
                rect.x * scaleFactor,
                rect.y * scaleFactor,
                rect.width * scaleFactor,
                rect.height * scaleFactor
            );
        }

        private Rect _rect;
        private Rect _scaledRect;

        /// <summary>
        /// The bounds of the cartography in the Level space.
        /// </summary>
        public Rect Rect => _rect;

        /// <summary>
        /// The size of the cartography in the Level space.
        /// </summary>
        public Vector2 Size => _rect.size;

        /// <summary>
        /// The center of the cartography in the Level space.
        /// </summary>
        public Vector2 Center => _rect.center;

        /// <summary>
        /// The minimum point of the cartography in the Level space.
        /// </summary>
        public Vector2 Min => _rect.min;

        /// <summary>
        /// The maximum point of the cartography in the Level space.
        /// </summary>
        public Vector2 Max => _rect.max;


        /// <summary>
        /// The bounds of the cartography in the Scaled space.
        /// </summary>
        public Rect ScaledRect => _scaledRect;

        /// <summary>
        /// The size of the cartography in the Scaled space.
        /// </summary>
        public Vector2 ScaledSize => _scaledRect.size;

        /// <summary>
        /// The center of the cartography in the Scaled space.
        /// </summary>
        public Vector2 ScaledCenter => _scaledRect.center;

        /// <summary>
        /// The minimum point of the cartography in the Scaled space.
        /// </summary>
        public Vector2 ScaledMin => _scaledRect.min;

        /// <summary>
        /// The maximum point of the cartography in the Scaled space.
        /// </summary>
        public Vector2 ScaledMax => _scaledRect.max;


        public CartographyBounds(CartographyBounds original)
        {
            _rect = original._rect;
            _scaledRect = original._scaledRect;
        }

        public CartographyBounds(Rect original, float scaleFactor)
        {
            _rect = original;
            _scaledRect = Scale(_rect, scaleFactor);
        }

        /// <summary>
        /// Expands this <see cref="CartographyBounds"/> to contain the given <paramref name="containedBounds"/>.
        /// </summary>
        /// <param name="containedBounds">The bounds to contain.</param>
        public void Expand(CartographyBounds containedBounds)
        {
            _rect = Rect.MinMaxRect(
                Mathf.Min(_rect.min.x, containedBounds.Rect.min.x),
                Mathf.Min(_rect.min.y, containedBounds.Rect.min.y),
                Mathf.Max(_rect.max.x, containedBounds.Rect.max.x),
                Mathf.Max(_rect.max.y, containedBounds.Rect.max.y)
            );

            _scaledRect = Rect.MinMaxRect(
                Mathf.Min(_scaledRect.min.x, containedBounds.ScaledRect.min.x),
                Mathf.Min(_scaledRect.min.y, containedBounds.ScaledRect.min.y),
                Mathf.Max(_scaledRect.max.x, containedBounds.ScaledRect.max.x),
                Mathf.Max(_scaledRect.max.y, containedBounds.ScaledRect.max.y)
            );
        }
    }
}