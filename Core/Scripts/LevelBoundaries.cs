using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
    public class LevelBoundaries : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private PolygonCollider2D _shape;

        #endregion

        #region Fields

        #endregion

        #region Getters

        public PolygonCollider2D Shape => _shape;
        public Bounds Bounds => _shape.bounds;
        public Vector2 Size => _shape.bounds.size;
        public Vector2 Center => _shape.bounds.center;
        public Vector2 Min => _shape.bounds.min;
        public Vector2 Max => _shape.bounds.max;

        #endregion

        #region Behaviour

        private void Awake()
        {
            Compose();
        }

        #endregion

        #region Preparing

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