using UnityEngine;
using UnityEngine.Events;

namespace LDtkLevelManager.Implementations.Basic
{
    /// <summary>
    /// A bridge between other entities and the <see cref="LevelTransitioner"/>. Use
    /// this to request level transitions.
    /// </summary>
    [CreateAssetMenu(fileName = "Level Transitioner Bridge", menuName = "LDtkLevelManager/Transitioning/Transitioner Bridge", order = 0)]
    public class LevelTransitionerBridge : ScriptableObject
    {
        #region Fields

        private LevelTransitioner _levelTransitioner;

        #endregion

        #region Registering

        /// <summary>
        /// Register a <see cref="LevelTransitioner"/> with this bridge. This is how you
        /// tell the bridge which <see cref="LevelTransitioner"/> to send your requests to.
        /// </summary>
        /// <param name="levelTransitioner">The <see cref="LevelTransitioner"/> to send your
        /// requests to.</param>
        public void Register(LevelTransitioner levelTransitioner)
        {
            _levelTransitioner = levelTransitioner;
        }

        /// <summary>
        /// Clear the <see cref="LevelTransitioner"/> registry. This is useful when you want to
        /// release any references to the <see cref="LevelTransitioner"/> that may have been
        /// registered.
        /// </summary>
        public void ClearRegistry()
        {
            _levelTransitioner = null;
        }

        #endregion

        #region Transitioning

        /// <summary>
        /// Request a level transition into a given spot.
        /// </summary>
        /// <param name="levelIid">The iid of the level to transition into.</param>
        /// <param name="spotIid">The iid of the <see cref="LDtkUnity.Spot"/> to transition into.</param>
        /// <remarks>
        /// If the <see cref="LevelTransitioner"/> has not been set yet, this method will do nothing.
        /// </remarks>
        public void TransitionIntoSpot(string levelIid, string spotIid)
        {
            if (_levelTransitioner == null) return;
            _levelTransitioner.TransitionToSpot(levelIid, spotIid);
        }

        /// <summary>
        /// Request a level transition into a given connection.
        /// </summary>
        /// <param name="levelIid">The iid of the level to transition into.</param>
        /// <param name="connection">The connection to transition from.</param>
        /// <remarks>
        /// If the <see cref="LevelTransitioner"/> has not been set yet, this method will do nothing.
        /// </remarks>
        public void TransitionToConnection(string levelIid, IConnection connection)
        {
            if (_levelTransitioner == null) return;
            _levelTransitioner.TransitionToConnection(levelIid, connection);
        }

        /// <summary>
        /// Request a level transition into a given portal.
        /// </summary>
        /// <param name="levelIid">The iid of the level to transition into.</param>
        /// <param name="portal">The portal to transition from.</param>
        /// <remarks>
        /// If the <see cref="LevelTransitioner"/> has not been set yet, this method will do nothing.
        /// </remarks>
        public void TransitionToPortal(string levelIid, IPortal portal)
        {
            if (_levelTransitioner == null) return;
            _levelTransitioner.TransitionToPortal(levelIid, portal);
        }

        #endregion
    }
}