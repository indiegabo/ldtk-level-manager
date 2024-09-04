using UnityEngine;
using UnityEngine.Events;

namespace LDtkLevelManager
{
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
        /// The event that is triggered when a level is exited.
        /// </summary>
        public UnityEvent<LevelBehaviour> LevelExitedEvent => _levelExitedEvent;

        /// <summary>
        /// The event that is triggered when a level is prepared.
        /// </summary>
        public UnityEvent<LevelBehaviour, LevelTrail> LevelPreparedEvent => _levelPreparedEvent;

        /// <summary>
        /// The event that is triggered when a level is entered.
        /// </summary>
        public UnityEvent<LevelBehaviour> LevelEnteredEvent => _levelEnteredEvent;

        #endregion
    }
}