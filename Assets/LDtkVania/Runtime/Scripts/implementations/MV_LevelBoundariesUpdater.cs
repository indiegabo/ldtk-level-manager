using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    [CreateAssetMenu(fileName = "Level Boundaries Updater", menuName = "LDtkVania/Levels/Boundaries Updater", order = 0)]
    public class MV_LevelBoundariesUpdater : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        private UnityEvent<PolygonCollider2D> _boundariesUpdated;

        #endregion

        #region Getters

        public UnityEvent<PolygonCollider2D> BoundariesUpdated => _boundariesUpdated;

        #endregion

        #region Updating 

        public void UpdateBoundaries(PolygonCollider2D boundaries)
        {
            _boundariesUpdated.Invoke(boundaries);
        }

        #endregion
    }
}