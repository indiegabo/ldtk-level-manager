using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
    [Tooltip("Enforces the geometry of the level. Only needed if the level has a scene. This solution is focused on levels wrapped in scenes.")]
    public class GeometryEnforcer : MonoBehaviour
    {
        #region Fields

        private LDtkIid _ldtkIid;
        private LevelInfo _info;

        #endregion

        #region Behaviour

        private void Awake()
        {
            if (!TryGetComponent(out _ldtkIid))
            {
                Logger.Error($"{name} has no LDtkIid component", this);
                return;
            }

            if (!LevelLoader.Instance.TryGetLevel(_ldtkIid.Iid, out _info))
            {
                var message = $"{name} could not have its geometry enforced because there was no level "
                    + $"found under the LDtk Iid {_ldtkIid.Iid}";
                Logger.Error(message, this);
                return;
            }

            // Only needed if the level has a scene. This solution is focused
            // on levels pre instantiated in scenes.
            if (!_info.HasScene)
            {
                Destroy(this);
                return;
            }

            Enforce();
            Destroy(this);
        }

        #endregion

        #region Geometry

        public void Enforce()
        {
            var colliders = GetComponentsInChildren<CompositeCollider2D>();

            foreach (var collider in colliders)
            {
                collider.GenerateGeometry();
            }
        }

        #endregion
    }
}