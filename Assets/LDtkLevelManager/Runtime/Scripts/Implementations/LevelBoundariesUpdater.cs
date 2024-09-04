using UnityEngine;
using UnityEngine.Events;

namespace LDtkLevelManager
{
    [CreateAssetMenu(fileName = "Level Boundaries Updater", menuName = "LDtkLevelManager/Levels/Boundaries Updater", order = 0)]
    public class LevelBoundariesUpdater : ScriptableObject
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