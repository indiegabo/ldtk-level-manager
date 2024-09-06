using LDtkUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDtkLevelManager
{
    [Tooltip("Enforces the geometry of the level. Only needed if the level has a scene. This solution is focused on levels wrapped in scenes.")]
    public class SceneLevelSetuper : MonoBehaviour
    {
        #region Fields

        private LDtkIid _ldtkIid;

        #endregion

        #region Behaviour

        private void Awake()
        {
            if (!TryGetComponent(out _ldtkIid))
            {
                Logger.Error($"{name} has no LDtkIid component", this);
                return;
            }

            if (!LevelLoader.Instance.TryGetLevel(_ldtkIid.Iid, out LevelInfo info))
            {
                var message = $"{name} could not have its geometry enforced because there was no level "
                    + $"found under the LDtk Iid {_ldtkIid.Iid}";
                Logger.Error(message, this);
                return;
            }

            Setup(info);
            Destroy(this);
        }

        #endregion

        #region Geometry

        public void Setup(LevelInfo levelInfo)
        {
            if (!levelInfo.HasScene)
            {
                return;
            }

            var tilemaps = GetComponentsInChildren<TilemapCollider2D>();

            foreach (var tilemapCollider in tilemaps)
            {
                if (tilemapCollider.hasTilemapChanges)
                {
                    tilemapCollider.ProcessTilemapChanges();
                }

                if (tilemapCollider.TryGetComponent(out CompositeCollider2D compositeCollider2D))
                {
                    compositeCollider2D.GenerateGeometry();
                }
            }
        }

        #endregion
    }
}