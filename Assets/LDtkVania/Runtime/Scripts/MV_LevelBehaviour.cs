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
    public class MV_LevelBehaviour : MonoBehaviour, ILDtkImportedLevel
    {
        #region Inspector

        [SerializeField]
        private string _ldtkLevelIid;

        [SerializeField]
        private MV_LevelManager _levelHandler;

        [SerializeField]
        private bool _waitOnCameraBlend = true;

        [SerializeField]
        private GameObjectProvider _mainCharacterProvider;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _exitedEvent;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour, MV_LevelConnection, MV_ICheckpoint> _preparationStartedEvent;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _preparedEvent;

        [SerializeField]
        private UnityEvent<MV_LevelBehaviour> _enteredEvent;

        #endregion

        #region Fields

        private CinemachineBrain _cinemachineBrain;
        private MV_Level _mvLevel;

        private MV_LevelDefaultSpawnPoint _defaultSpawnPoint;
        private MV_LevelConnection _currentPreparedConnection;

        private bool _waitingOnBlend = false;

        private List<MV_LevelConnection> _connections;
        private Dictionary<string, MV_LevelConnection> _connectionsDictionary;

        private List<MV_ICheckpoint> _checkpoints;
        private Dictionary<string, MV_ICheckpoint> _checkpointsDictionary;

        #endregion

        #region Getters

        public UnityEvent<MV_LevelBehaviour> ExitedEvent => _exitedEvent;
        public UnityEvent<MV_LevelBehaviour, MV_LevelConnection, MV_ICheckpoint> PreparationStartedEvent => _preparationStartedEvent;
        public UnityEvent<MV_LevelBehaviour> PreparedEvent => _preparedEvent;
        public UnityEvent<MV_LevelBehaviour> EnteredEvent => _enteredEvent;

        #endregion

        #region Behaviour

        private void Awake()
        {
            if (!MV_LevelManager.Instance.TryGetLevel(_ldtkLevelIid, out _mvLevel))
            {
                MV_Logger.Error($"{name} could not be activated because {_ldtkLevelIid} is not present on dictionary", this);
                return;
            }

            BroadcastMessage("OnLevelAwake", this, SendMessageOptions.DontRequireReceiver);

            if (Camera.main != null)
                _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

            EvaluateConnections();
            EvaluateCheckpoints();

            MV_LevelManager.Instance.RegisterAsBehaviour(_ldtkLevelIid, this);
        }

        private void OnDestroy()
        {
            MV_LevelManager.Instance.UnregisterAsBehaviour(_ldtkLevelIid);
        }

        #endregion

        #region Level Cycle

        /// <summary>
        /// Prepares the cycle. This should be triggered while curtains are closed
        /// </summary>
        /// <param name="trail"></param>
        /// <returns></returns>
        public async Task Prepare(MV_LevelTrail trail = null)
        {
            _currentPreparedConnection = null;

            if (trail == null)
            {
                await PerformPreparation(_defaultSpawnPoint);
            }
            else if (trail != null && string.IsNullOrEmpty(trail.ConnectionKey))
            {
                MV_Logger.Warning($"{name} preparing level but trail has no connection key. Using default spawn point", this);
                await PerformPreparation(_defaultSpawnPoint);
            }
            else if (!_connectionsDictionary.ContainsKey(trail.ConnectionKey))
            {
                MV_Logger.Warning($"{name} does not have a connection under {trail.ConnectionKey} key. Using default spawn point", this);
                await PerformPreparation(_defaultSpawnPoint);
            }
            else
            {
                _currentPreparedConnection = _connectionsDictionary[trail.ConnectionKey];
                await PerformPreparation(_currentPreparedConnection);
            }
        }

        public async Task Prepare(MV_ICheckpoint checkpoint)
        {
            if (_checkpointsDictionary.TryGetValue(checkpoint.Iid, out MV_ICheckpoint registeredCheckpoint))
            {
                await PerformPreparation(registeredCheckpoint);
            }
            else
            {
                MV_Logger.Warning($"{name} does not have a checkpoint under {checkpoint.Iid} key. Using default spawn point", this);
                await PerformPreparation(_defaultSpawnPoint);
            }
        }

        private async Task PerformPreparation(MV_LevelDefaultSpawnPoint spawnPoint)
        {
            _preparationStartedEvent.Invoke(this, null, null);
            SpawnCharacter(spawnPoint.transform.position, spawnPoint.DirectionSign);
            await WaitOnCameraBlend();
            _preparedEvent.Invoke(this);
        }

        private async Task PerformPreparation(MV_LevelConnection connection)
        {
            _preparationStartedEvent.Invoke(this, connection, null);
            SpawnCharacter(connection.Trail.SpawnPosition, connection.DirectionSign);
            await WaitOnCameraBlend();
            _preparedEvent.Invoke(this);
        }

        private async Task PerformPreparation(MV_ICheckpoint checkpoint)
        {
            _preparationStartedEvent.Invoke(this, null, checkpoint);
            SpawnCharacter(checkpoint.SpawnPosition, checkpoint.DirectionSign);
            await WaitOnCameraBlend();
            _preparedEvent.Invoke(this);
        }

        /// <summary>
        /// Enters the level activating its subjects
        /// </summary>
        /// <returns></returns>
        public MV_LevelTrail Enter()
        {
            EnableConnections();
            MV_LevelTrail trail = new();

            if (_currentPreparedConnection != null)
            {
                trail = _currentPreparedConnection.Trail;
            }
            else
            {
                trail.SpawnPosition = _defaultSpawnPoint.transform.position;
            }

            _enteredEvent.Invoke(this);
            return trail;
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
            _connectionsDictionary = new Dictionary<string, MV_LevelConnection>();

            Transform connectionsContainer = transform.Find(MV_LevelManager.Instance.ConnectionsContainerName);

            if (connectionsContainer == null) return;

            _defaultSpawnPoint = connectionsContainer.GetComponentInChildren<MV_LevelDefaultSpawnPoint>();

            _connections = connectionsContainer.GetComponentsInChildren<MV_LevelConnection>().ToList();

            foreach (MV_LevelConnection connection in _connections)
            {
                _connectionsDictionary.Add(connection.Trail.ConnectionKey, connection);
                connection.Deactivate();
            }
        }

        private void EnableConnections()
        {
            if (_connections == null) return;

            foreach (MV_LevelConnection connection in _connections)
            {
                connection.Activate();
            }
        }

        private void DisableConnections()
        {
            if (_connections == null) return;

            foreach (MV_LevelConnection connection in _connections)
            {
                connection.Deactivate();
            }
        }

        #endregion

        #region Checkpoints

        private void EvaluateCheckpoints()
        {
            _checkpointsDictionary = new Dictionary<string, MV_ICheckpoint>();

            Transform checkpointsContainer = transform.Find(MV_LevelManager.Instance.CheckpointsContainerName);

            if (checkpointsContainer == null) return;

            _checkpoints = checkpointsContainer.GetComponentsInChildren<MV_ICheckpoint>().ToList();

            foreach (MV_ICheckpoint checkpoint in _checkpoints)
            {
                _checkpointsDictionary.Add(checkpoint.Iid, checkpoint);
            }
        }

        #endregion

        #region Main Character

        private void SpawnCharacter(Vector2 position, int directionSign)
        {
            if (!_mainCharacterProvider.TryGetComponent(out IMetroidvaniaLevelSpawnSubject spawnSubject))
            {
                MV_Logger.Error($"{name} could not find an ({nameof(IMetroidvaniaLevelSpawnSubject)}) to spawn", this);
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

        #region LDtk   

        public void OnLDtkImportLevel(Level level)
        {
            _ldtkLevelIid = level.Iid;
        }

        #endregion
    }
}