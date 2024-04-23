using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    public class MV_LevelSubject : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _levelSet;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _exited;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour, MV_LevelConnection, MV_ICheckpoint> _preparationStarted;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _prepared;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _entered;


        #endregion

        #region Fields

        private MV_LevelBehaviour _levelBehaviour;

        #endregion

        #region Getters

        public bool HasBehaviour => _levelBehaviour != null;
        private MV_LevelBehaviour LevelBehaviour => _levelBehaviour;
        public UnityEvent<MV_LevelBehaviour> LevelSet => _levelSet;

        public UnityEvent<MV_LevelBehaviour> Exited => _exited;
        public UnityEvent<MV_LevelBehaviour, MV_LevelConnection, MV_ICheckpoint> PreparationStarted => _preparationStarted;
        public UnityEvent<MV_LevelBehaviour> Prepared => _prepared;
        public UnityEvent<MV_LevelBehaviour> Entered => _entered;

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

        private void OnLevelAwake(MV_LevelBehaviour levelBehaviour)
        {
            _levelBehaviour = levelBehaviour;
            _levelSet.Invoke(_levelBehaviour);
            RegisterEvents(_levelBehaviour);
        }

        #endregion

        #region Level Callbacks

        private void OnLevelExited(MV_LevelBehaviour behaviour)
        {
            _exited.Invoke(behaviour);
        }

        private void OnLevelPreparationStarted(MV_LevelBehaviour behaviour, MV_LevelConnection connection, MV_ICheckpoint checkpointData)
        {
            _preparationStarted.Invoke(behaviour, connection, checkpointData);
        }

        private void OnLevelPrepared(MV_LevelBehaviour behaviour)
        {
            _prepared.Invoke(behaviour);
        }

        private void OnLevelEntered(MV_LevelBehaviour behaviour)
        {
            _entered.Invoke(behaviour);
        }

        #endregion

        #region Events

        private void RegisterEvents(MV_LevelBehaviour behaviour)
        {
            if (behaviour == null) return;

            _levelBehaviour.ExitedEvent.AddListener(OnLevelExited);
            _levelBehaviour.PreparationStartedEvent.AddListener(OnLevelPreparationStarted);
            _levelBehaviour.PreparedEvent.AddListener(OnLevelPrepared);
            _levelBehaviour.EnteredEvent.AddListener(OnLevelEntered);
        }

        private void UnregisterEvents(MV_LevelBehaviour behaviour)
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