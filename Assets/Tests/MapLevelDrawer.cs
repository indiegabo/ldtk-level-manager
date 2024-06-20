using UnityEngine;
using LDtkVania.Cartography;

namespace Tests
{
    public class MapLevelDrawer : MonoBehaviour
    {
        #region Inspector

        #endregion

        #region Fields

        private SpriteRenderer _renderer;

        #endregion

        #region Behaviour

        #endregion

        #region Initializing

        public void Initialize(LevelCartography levelCartography, float zDepth)
        {
            _renderer = GetComponent<SpriteRenderer>();
            name = $"{levelCartography.Name}";

            Vector2 center = levelCartography.Bounds.ScaledCenter;
            transform.position = new Vector3(center.x, center.y, zDepth);
            _renderer.size = levelCartography.Bounds.ScaledSize;
        }

        #endregion
    }
}