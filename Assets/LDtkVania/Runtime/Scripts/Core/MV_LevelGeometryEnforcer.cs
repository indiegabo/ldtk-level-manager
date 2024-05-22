using LDtkUnity;
using UnityEngine;
using System.Linq;

namespace LDtkVania
{
    public class MV_LevelGeometryEnforcer : MonoBehaviour
    {
        #region Fields

        private LDtkIid _ldtkIid;
        private MV_Level _mvLevel;

        #endregion

        #region Behaviour

        private void Awake()
        {
            if (!TryGetComponent(out _ldtkIid))
            {
                MV_Logger.Error($"{name} has no LDtkIid component", this);
                return;
            }

            if (!MV_LevelManager.Instance.TryGetLevel(_ldtkIid.Iid, out _mvLevel))
            {
                MV_Logger.Error($"{name} could not be activated because {_ldtkIid.Iid} is not present on dictionary", this);
                return;
            }

            HandleLevelGeometry(_mvLevel);
        }

        #endregion

        #region Geometry

        private void HandleLevelGeometry(MV_Level level)
        {
            if (!level.HasScene) return;

            var colliders = GetComponentsInChildren<CompositeCollider2D>();

            foreach (var collider in colliders)
            {
                collider.GenerateGeometry();
            }
        }

        #endregion
    }
}