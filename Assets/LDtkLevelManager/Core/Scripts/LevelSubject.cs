using UnityEngine;
using UnityEngine.Events;

namespace LDtkLevelManager
{
    /// <summary>
    /// A MonoBehaviour that listens to the events from a <see cref="UniverseLevelBehaviour"/> and 
    /// dispatches them to its subscribers. Awesome for entities prefabs.
    /// </summary>
    public class LevelSubject : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private UnityEvent<UniverseLevelBehaviour> _levelSet;

        [SerializeField]
        private UnityEvent<UniverseLevelBehaviour> _exited;

        [SerializeField]
        private UnityEvent<UniverseLevelBehaviour, ILevelFlowSubject, Vector2> _preparationStarted;

        [SerializeField]
        private UnityEvent<UniverseLevelBehaviour, ILevelFlowSubject, LevelTrail> _prepared;

        [SerializeField]
        private UnityEvent<UniverseLevelBehaviour> _entered;


        #endregion

        #region Fields

        private UniverseLevelBehaviour _levelBehaviour;

        #endregion

        #region Getters

        /// <summary>
        /// Gets a value indicating whether this instance has a level behaviour.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a level behaviour; otherwise, <c>false</c>.
        /// </value>
        public bool HasBehaviour => _levelBehaviour != null;

        /// <summary>
        /// Occurs when the level set property has changed.
        /// </summary>
        public UnityEvent<UniverseLevelBehaviour> LevelSet => _levelSet;

        /// <summary>
        /// Occurs when the player exited the level.
        /// </summary>
        public UnityEvent<UniverseLevelBehaviour> Exited => _exited;

        /// <summary>
        /// Occurs when the level has started preparation. Meaning the player will be
        /// spawned in the level.
        /// </summary>
        public UnityEvent<UniverseLevelBehaviour, ILevelFlowSubject, Vector2> PreparationStarted => _preparationStarted;

        /// <summary>
        /// Occurs when the level has finished preparation. Meaning the player is in the 
        /// correct spot in the level and the curtains are about to be opened.
        /// </summary>
        public UnityEvent<UniverseLevelBehaviour, ILevelFlowSubject, LevelTrail> Prepared => _prepared;

        /// <summary>
        /// Occurs when the level has finished entering. Curtains are now open and the
        /// gameplay is restablished.
        /// </summary>
        public UnityEvent<UniverseLevelBehaviour> Entered => _entered;

        #endregion

        #region Behaviour

        private void OnEnable()
        {
            RegisterEvents(_levelBehaviour);
        }

        private void OnDisable()
        {
            UnregisterEvents(_levelBehaviour);
        }

        #endregion

        #region Metroidvania level

        private void OnLevelAwake(UniverseLevelBehaviour levelBehaviour)
        {
            _levelBehaviour = levelBehaviour;
            _levelSet.Invoke(_levelBehaviour);
            RegisterEvents(_levelBehaviour);
        }

        #endregion

        #region Level Callbacks

        private void OnLevelExited(UniverseLevelBehaviour behaviour)
        {
            _exited.Invoke(behaviour);
        }

        private void OnLevelPreparationStarted(UniverseLevelBehaviour behaviour, ILevelFlowSubject subject, Vector2 point)
        {
            _preparationStarted.Invoke(behaviour, subject, point);
        }

        private void OnLevelPrepared(UniverseLevelBehaviour behaviour, ILevelFlowSubject subject, LevelTrail trail)
        {
            _prepared.Invoke(behaviour, subject, trail);
        }

        private void OnLevelEntered(UniverseLevelBehaviour behaviour)
        {
            _entered.Invoke(behaviour);
        }

        #endregion

        #region Events

        private void RegisterEvents(UniverseLevelBehaviour behaviour)
        {
            if (behaviour == null) return;

            _levelBehaviour.Deactivated.AddListener(OnLevelExited);
            _levelBehaviour.PreparationStarted.AddListener(OnLevelPreparationStarted);
            _levelBehaviour.Prepared.AddListener(OnLevelPrepared);
            _levelBehaviour.Activated.AddListener(OnLevelEntered);
        }

        private void UnregisterEvents(UniverseLevelBehaviour behaviour)
        {
            if (behaviour == null) return;

            _levelBehaviour.Deactivated.RemoveListener(OnLevelExited);
            _levelBehaviour.PreparationStarted.RemoveListener(OnLevelPreparationStarted);
            _levelBehaviour.Prepared.RemoveListener(OnLevelPrepared);
            _levelBehaviour.Activated.RemoveListener(OnLevelEntered);
        }

        #endregion
    }
}