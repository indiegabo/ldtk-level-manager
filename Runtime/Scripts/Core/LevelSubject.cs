using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    public class LevelSubject : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private UnityEvent<LevelBehaviour> _levelSet;

        [SerializeField]
        private UnityEvent<LevelBehaviour> _exited;

        [SerializeField]
        private UnityEvent<LevelBehaviour, Vector2> _preparationStarted;

        [SerializeField]
        private UnityEvent<LevelBehaviour, LevelTrail> _prepared;

        [SerializeField]
        private UnityEvent<LevelBehaviour> _entered;


        #endregion

        #region Fields

        private LevelBehaviour _levelBehaviour;

        #endregion

        #region Getters

        public bool HasBehaviour => _levelBehaviour != null;
        private LevelBehaviour LevelBehaviour => _levelBehaviour;
        public UnityEvent<LevelBehaviour> LevelSet => _levelSet;

        public UnityEvent<LevelBehaviour> Exited => _exited;
        public UnityEvent<LevelBehaviour, Vector2> PreparationStarted => _preparationStarted;
        public UnityEvent<LevelBehaviour, LevelTrail> Prepared => _prepared;
        public UnityEvent<LevelBehaviour> Entered => _entered;

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

        private void OnLevelAwake(LevelBehaviour levelBehaviour)
        {
            _levelBehaviour = levelBehaviour;
            _levelSet.Invoke(_levelBehaviour);
            RegisterEvents(_levelBehaviour);
        }

        #endregion

        #region Level Callbacks

        private void OnLevelExited(LevelBehaviour behaviour)
        {
            _exited.Invoke(behaviour);
        }

        private void OnLevelPreparationStarted(LevelBehaviour behaviour, Vector2 point)
        {
            _preparationStarted.Invoke(behaviour, point);
        }

        private void OnLevelPrepared(LevelBehaviour behaviour, LevelTrail trail)
        {
            _prepared.Invoke(behaviour, trail);
        }

        private void OnLevelEntered(LevelBehaviour behaviour)
        {
            _entered.Invoke(behaviour);
        }

        #endregion

        #region Events

        private void RegisterEvents(LevelBehaviour behaviour)
        {
            if (behaviour == null) return;

            _levelBehaviour.ExitedEvent.AddListener(OnLevelExited);
            _levelBehaviour.PreparationStartedEvent.AddListener(OnLevelPreparationStarted);
            _levelBehaviour.PreparedEvent.AddListener(OnLevelPrepared);
            _levelBehaviour.EnteredEvent.AddListener(OnLevelEntered);
        }

        private void UnregisterEvents(LevelBehaviour behaviour)
        {
            if (behaviour == null) return;

            _levelBehaviour.ExitedEvent.RemoveListener(OnLevelExited);
            _levelBehaviour.PreparationStartedEvent.RemoveListener(OnLevelPreparationStarted);
            _levelBehaviour.PreparedEvent.RemoveListener(OnLevelPrepared);
            _levelBehaviour.EnteredEvent.RemoveListener(OnLevelEntered);
        }

        #endregion
    }
}