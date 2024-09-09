using UnityEngine;
using LDtkLevelManager.Cartography;

namespace LDtkLevelManager.Implementations.Basic
{
    public class MapLevelDrawer : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private Sprite _drawingSprite;

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

            _renderer.sprite = _drawingSprite;
            _renderer.drawMode = SpriteDrawMode.Sliced;

            Vector2 center = levelCartography.Bounds.ScaledCenter;
            transform.position = new Vector3(center.x, center.y, zDepth);
            _renderer.size = levelCartography.Bounds.ScaledSize;
        }

        #endregion
    }
}