using UnityEngine;
using UnityEngine.Events;

namespace LDtkLevelManager
{
    /// <summary>
    /// Used by the <see cref="LDtkLevelManager.LevelLoader"/> to fire events about the
    /// player's navigation through the Universe and its levels.
    /// </summary>
    [CreateAssetMenu(fileName = "Level Navigation Bridge", menuName = "LDtkLevelManager/Navigation/Navigation Bridge", order = 0)]
    public class LevelNavigationBridge : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        private UnityEvent<LevelBehaviour> _levelExitedEvent;

        [SerializeField]
        private UnityEvent<LevelBehaviour, LevelTrail> _levelPreparedEvent;

        [SerializeField]
        private UnityEvent<LevelBehaviour> _levelEnteredEvent;

        #endregion

        #region Getters

        /// <summary>
        /// Occurs when the player has exited a level.
        /// </summary>
        public UnityEvent<LevelBehaviour> PlayerExitedLevel => _levelExitedEvent;

        /// <summary>
        /// Occurs when a level is prepared for the player to enter. The curtains are
        /// still closed an about to be opened.
        /// </summary>
        public UnityEvent<LevelBehaviour, LevelTrail> LevelPrepared => _levelPreparedEvent;

        /// <summary>
        /// Occurs when the player has entered a level and the curtains have been
        /// opened.
        /// </summary>
        public UnityEvent<LevelBehaviour> PlayerEnteredLevel => _levelEnteredEvent;

        #endregion
    }
}