using UnityEngine;
using UnityEngine.Events;

namespace LDtkLevelManager.Implementations.Basic
{
    [CreateAssetMenu(fileName = "Level Transitioner Bridge", menuName = "LDtkLevelManager/Transitioning/Transitioner Bridge", order = 0)]
    public class LevelTransitionerBridge : ScriptableObject
    {
        #region Fields

        private LevelTransitioner _levelTransitioner;

        #endregion

        #region Registering

        public void Register(LevelTransitioner levelTransitioner)
        {
            _levelTransitioner = levelTransitioner;
        }

        public void ClearRegistry()
        {
            _levelTransitioner = null;
        }

        #endregion

        #region Transitioning

        public void TransitionIntoSpot(string levelIid, string spotIid)
        {
            if (_levelTransitioner == null) return;
            _levelTransitioner.TransitionIntoSpot(levelIid, spotIid);
        }

        public void TransitionToConnection(string levelIid, IConnection connection)
        {
            if (_levelTransitioner == null) return;
            _levelTransitioner.TransitionToConnection(levelIid, connection);
        }

        public void TransitionToPortal(string levelIid, IPortal portal)
        {
            if (_levelTransitioner == null) return;
            _levelTransitioner.TransitionToPortal(levelIid, portal);
        }

        #endregion
    }
}