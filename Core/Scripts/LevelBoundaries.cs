using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
    /// <summary>
    /// The boundaries of the level.
    /// </summary>
    public class LevelBoundaries : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private PolygonCollider2D _shape;

        #endregion

        #region Fields

        #endregion

        #region Getters

        /// <summary>
        /// The <see cref="PolygonCollider2D"/> representing shape of the level boundaries.
        /// </summary>
        public PolygonCollider2D Shape => _shape;

        /// <summary>
        /// The bounds of the level boundaries.
        /// </summary>
        public Bounds Bounds => _shape.bounds;

        /// <summary>
        /// The size of the level boundaries.
        /// </summary>
        public Vector2 Size => _shape.bounds.size;

        /// <summary>
        /// The center of the level boundaries.
        /// </summary>
        public Vector2 Center => _shape.bounds.center;

        /// <summary>
        /// The minimum point of the level boundaries.
        /// </summary>
        public Vector2 Min => _shape.bounds.min;

        /// <summary>
        /// The maximum point of the level boundaries.
        /// </summary>
        public Vector2 Max => _shape.bounds.max;

        #endregion

        #region Behaviour

        private void Awake()
        {
            Compose();
        }

        #endregion

        #region Preparing

        /// <summary>
        /// Composes the level boundaries shape.
        /// </summary>
        /// <remarks>
        /// This will create a square shape with the size of the level.
        /// The points are set in the order of top-right, top-left, bottom-left, bottom-right.
        /// </remarks>
        public void Compose()
        {
            var ldtkComponentLevel = GetComponent<LDtkComponentLevel>();
            Vector2 size = ldtkComponentLevel.Size;
            _shape.points = new Vector2[] {
                new(size.x, size.y),
                new(0, size.y),
                new(0, 0),
                new(size.x, 0)
            };
        }

        #endregion
    }
}