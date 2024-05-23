using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    [CreateAssetMenu(fileName = "Level Navigation Bridge", menuName = "LDtkVania/Navigation/Navigation Bridge", order = 0)]
    public class MV_LevelNavigationBridge : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _levelExitedEvent;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour, MV_LevelTrail> _levelPreparedEvent;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _levelEnteredEvent;

        #endregion

        #region Getters

        /// <summary>
        /// The event that is triggered when a level is exited.
        /// </summary>
        public UnityEvent<MV_LevelBehaviour> LevelExitedEvent => _levelExitedEvent;

        /// <summary>
        /// The event that is triggered when a level is prepared.
        /// </summary>
        public UnityEvent<MV_LevelBehaviour, MV_LevelTrail> LevelPreparedEvent => _levelPreparedEvent;

        /// <summary>
        /// The event that is triggered when a level is entered.
        /// </summary>
        public UnityEvent<MV_LevelBehaviour> LevelEnteredEvent => _levelEnteredEvent;

        #endregion
    }
}