using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cinemachine;
using LDtkUnity;
using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    public class MV_LevelBehaviour : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private bool _waitOnCameraBlend = true;

        [SerializeField]
        private GameObjectProvider _mainCharacterProvider;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _exitedEvent;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour, Vector2> _preparationStartedEvent;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour, MV_LevelTrail> _preparedEvent;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _enteredEvent;

        #endregion

        #region Fields

        private LDtkIid _ldtkIid;

        private CinemachineBrain _cinemachineBrain;
        private MV_Level _mvLevel;

        private MV_LevelDefaultSpawnPoint _defaultSpawnPoint;

        private bool _waitingOnBlend = false;

        private Dictionary<string, MV_IConnection> _connections;
        private Dictionary<string, MV_ICheckpoint> _checkpoints;

        #endregion

        #region Getters

        public UnityEvent<MV_LevelBehaviour> ExitedEvent => _exitedEvent;
        public UnityEvent<MV_LevelBehaviour, Vector2> PreparationStartedEvent => _preparationStartedEvent;
        public UnityEvent<MV_LevelBehaviour, MV_LevelTrail> PreparedEvent => _preparedEvent;
        public UnityEvent<MV_LevelBehaviour> EnteredEvent => _enteredEvent;

        #endregion

        #region Behaviour

        private void Awake()
        {
            if (!TryGetComponent(out _ldtkIid))
            {
                MV_Logger.Error($"{name} has no LDtkIid component", this);
                return;
            }

            if (!MV_LevelManager.Instance.TryGetLevel(_ldtkIid.Iid, out _mvLevel))
            {
                MV_Logger.Error($"{name} could not be activated because {_ldtkIid.Iid} is not present on dictionary", this);
                return;
            }

            BroadcastMessage("OnLevelAwake", this, SendMessageOptions.DontRequireReceiver);

            if (Camera.main != null)
                _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

            EvaluateConnections();
            EvaluateCheckpoints();

            MV_LevelManager.Instance.RegisterAsBehaviour(_ldtkIid.Iid, this);
        }

        private void OnDestroy()
        {
            MV_LevelManager.Instance.UnregisterAsBehaviour(_ldtkIid.Iid);
        }

        #endregion

        #region Level Cycle

        /// <summary>
        /// Prepares the cycle. This should be triggered while curtains are closed
        /// </summary>
        /// <param name="trail"></param>
        /// <returns></returns>
        public async Task Prepare()
        {
            if (_defaultSpawnPoint == null)
            {
                MV_Logger.Error($"{name} has no default spawn point", this);
                return;
            }

            Vector2 point = _defaultSpawnPoint.transform.position;
            _preparationStartedEvent.Invoke(this, point);
            SpawnCharacter(point, _defaultSpawnPoint.DirectionSign);
            await WaitOnCameraBlend();
            _preparedEvent.Invoke(this, MV_LevelTrail.FromPoint(point));

        }

        public async Task Prepare(MV_ICheckpoint checkpoint)
        {
            if (!_checkpoints.TryGetValue(checkpoint.Iid, out MV_ICheckpoint registeredCheckpoint))
            {
                MV_Logger.Error($"{name} could not be prepared because {checkpoint.Iid} is not present on dictionary", this);
                return;
            }

            _preparationStartedEvent.Invoke(this, registeredCheckpoint.SpawnPosition);
            SpawnCharacter(registeredCheckpoint.SpawnPosition, registeredCheckpoint.DirectionSign);
            await WaitOnCameraBlend();
            _preparedEvent.Invoke(this, MV_LevelTrail.FromCheckpoint(checkpoint));
        }

        public async Task Prepare(MV_IConnection connection)
        {
            if (!_checkpoints.TryGetValue(connection.Key, out MV_ICheckpoint registeredCheckpoint))
            {
                MV_Logger.Error($"{name} could not be prepared because the connection key \"{connection.Key}\" is not present on dictionary", this);
                return;
            }

            _preparationStartedEvent.Invoke(this, connection.SpawnPosition);
            SpawnCharacter(connection.SpawnPosition, connection.DirectionSign);
            await WaitOnCameraBlend();
            _preparedEvent.Invoke(this, MV_LevelTrail.FromConnection(connection));
        }

        /// <summary>
        /// Enters the level activating its subjects
        /// </summary>
        /// <returns></returns>
        public void Enter()
        {
            EnableConnections();
            _enteredEvent.Invoke(this);
        }

        /// <summary>
        /// Exits the level
        /// </summary>
        public void Exit()
        {
            DisableConnections();
            _exitedEvent.Invoke(this);
        }

        #endregion

        #region Connections

        private void EvaluateConnections()
        {
            _connections = new Dictionary<string, MV_IConnection>();

            string connectionsContainerName = MV_LevelManager.Instance.ConnectionsContainerName;

            Transform connectionsContainer = !string.IsNullOrEmpty(connectionsContainerName)
                ? transform.Find(connectionsContainerName)
                : transform;

            _defaultSpawnPoint = connectionsContainer.GetComponentInChildren<MV_LevelDefaultSpawnPoint>();

            MV_IConnection[] connectionsComponents = connectionsContainer.GetComponentsInChildren<MV_IConnection>();

            foreach (MV_IConnection connection in connectionsComponents)
            {
                if (_connections.ContainsKey(connection.Key))
                {
                    MV_Logger.Warning($"{name} has more than one connection with the same key: {connection.Key}. Using the first found", this);
                }
                _connections.Add(connection.Key, connection);
                connection.Deactivate();
            }
        }

        private void EnableConnections()
        {
            if (_connections == null) return;

            foreach (MV_IConnection connection in _connections.Values)
            {
                connection.Activate();
            }
        }

        private void DisableConnections()
        {
            if (_connections == null) return;

            foreach (MV_IConnection connection in _connections.Values)
            {
                connection.Deactivate();
            }
        }

        #endregion

        #region Checkpoints

        private void EvaluateCheckpoints()
        {
            _checkpoints = new Dictionary<string, MV_ICheckpoint>();

            Transform checkpointsContainer = transform.Find(MV_LevelManager.Instance.CheckpointsContainerName);

            if (checkpointsContainer == null) return;

            MV_ICheckpoint[] checkpointsComponents = checkpointsContainer.GetComponentsInChildren<MV_ICheckpoint>();

            foreach (MV_ICheckpoint checkpoint in checkpointsComponents)
            {
                if (_checkpoints.ContainsKey(checkpoint.Iid))
                {
                    MV_Logger.Warning($"{name} has more than one checkpoint with the same key: {checkpoint.Iid}. Using the first found", this);
                }

                _checkpoints.Add(checkpoint.Iid, checkpoint);
            }
        }

        #endregion

        #region Main Character

        private void SpawnCharacter(Vector2 position, int directionSign)
        {
            if (!_mainCharacterProvider.TryGetComponent(out MV_ILevelSpawnSubject spawnSubject))
            {
                MV_Logger.Error($"{name} could not find an ({nameof(MV_ILevelSpawnSubject)}) to spawn", this);
                return;
            }

            spawnSubject.Spawn(position, directionSign);
        }

        #endregion

        #region Camera

        private async Task WaitOnCameraBlend()
        {
            if (!_waitOnCameraBlend) return;

            _waitingOnBlend = true;

            StartCoroutine(DelayForCameraBlendDuration());

            while (_waitingOnBlend)
            {
                await Task.Yield();
            }
        }


        private IEnumerator DelayForCameraBlendDuration()
        {
            if (_cinemachineBrain == null)
            {
                _waitingOnBlend = false;
                yield break;
            }

            yield return Coroutines.GetWaitForSeconds(0.5f);

            CinemachineBlend blend = _cinemachineBrain.ActiveBlend;

            if (blend != null)
            {
                float duration = blend.Duration - 0.5f;

                if (duration > 0)
                {
                    yield return Coroutines.GetWaitForSeconds(duration);
                }
            }

            _waitingOnBlend = false;
        }

        #endregion
    }
}