using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cinemachine;
using LDtkUnity;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace LDtkVania
{
    public class MV_LevelBehaviour : MonoBehaviour
    {
        #region Inspector

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
        private MV_Level _mvLevel;

        private MV_LevelDefaultSpawnPoint _defaultSpawnPoint;

        private Dictionary<string, IConnection> _connections;
        private Dictionary<string, ILevelAnchor> _checkpoints;

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

            InitializeConnections();
            EvaluateCheckpoints();

            MV_LevelManager.Instance.RegisterAsBehaviour(_ldtkIid.Iid, this);
        }

        private void OnDestroy()
        {
            MV_LevelManager.Instance.UnregisterAsBehaviour(_ldtkIid.Iid);
        }

        #endregion

        #region Level Cycle

        public void Prepare()
        {
            if (_defaultSpawnPoint == null)
            {
                MV_Logger.Error($"{name} has no default spawn point", this);
                return;
            }

            Vector2 point = _defaultSpawnPoint.transform.position;
            _preparationStartedEvent.Invoke(this, point);
            SpawnCharacter(point, _defaultSpawnPoint.DirectionSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromPoint(point));
        }

        public void Prepare(ILevelAnchor checkpoint)
        {
            if (!_checkpoints.TryGetValue(checkpoint.AnchorIid, out ILevelAnchor registeredCheckpoint))
            {
                MV_Logger.Error($"{name} could not be prepared because {checkpoint.AnchorIid} is not present on dictionary", this);
                return;
            }

            _preparationStartedEvent.Invoke(this, registeredCheckpoint.SpawnPoint);
            SpawnCharacter(registeredCheckpoint.SpawnPoint, registeredCheckpoint.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromCheckpoint(checkpoint));
        }

        public void Prepare(IConnection connection)
        {
            if (!_connections.TryGetValue(connection.Key, out IConnection registeredConnection))
            {
                MV_Logger.Error($"{name} could not be prepared because the connection key \"{connection.Key}\" is not present on dictionary", this);
                return;
            }

            _preparationStartedEvent.Invoke(this, registeredConnection.SpawnPoint);
            SpawnCharacter(registeredConnection.SpawnPoint, registeredConnection.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromConnection(registeredConnection));
        }

        public void Enter()
        {
            EnableConnections();
            _enteredEvent.Invoke(this);
        }

        public void Exit()
        {
            DisableConnections();
            _exitedEvent.Invoke(this);
        }

        #endregion

        #region Connections

        private void InitializeConnections()
        {
            _connections = new Dictionary<string, IConnection>();

            string connectionsContainerName = MV_LevelManager.Instance.ConnectionsContainerName;

            Transform connectionsContainer = !string.IsNullOrEmpty(connectionsContainerName)
                ? transform.Find(connectionsContainerName)
                : transform;

            if (connectionsContainer == null) return;

            _defaultSpawnPoint = connectionsContainer.GetComponentInChildren<MV_LevelDefaultSpawnPoint>();

            IConnection[] connectionsComponents = connectionsContainer.GetComponentsInChildren<IConnection>();

            foreach (IConnection connection in connectionsComponents)
            {
                connection.Initialize();
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

            foreach (IConnection connection in _connections.Values)
            {
                connection.Activate();
            }
        }

        private void DisableConnections()
        {
            if (_connections == null) return;

            foreach (IConnection connection in _connections.Values)
            {
                connection.Deactivate();
            }
        }

        #endregion

        #region Checkpoints

        private void EvaluateCheckpoints()
        {
            _checkpoints = new Dictionary<string, ILevelAnchor>();

            Transform checkpointsContainer = transform.Find(MV_LevelManager.Instance.CheckpointsContainerName);

            if (checkpointsContainer == null) return;

            ILevelAnchor[] checkpointsComponents = checkpointsContainer.GetComponentsInChildren<ILevelAnchor>();

            foreach (ILevelAnchor checkpoint in checkpointsComponents)
            {
                if (_checkpoints.ContainsKey(checkpoint.AnchorIid))
                {
                    MV_Logger.Warning($"{name} has more than one checkpoint with the same key: {checkpoint.AnchorIid}. Using the first found", this);
                }

                _checkpoints.Add(checkpoint.AnchorIid, checkpoint);
            }
        }

        #endregion

        #region Main Character

        private void SpawnCharacter(Vector2 position, int directionSign)
        {
            if (!_mainCharacterProvider.TryGetComponent(out ILevelSpawnSubject spawnSubject))
            {
                MV_Logger.Error($"{name} could not find an ({nameof(ILevelSpawnSubject)}) to spawn", this);
                return;
            }

            spawnSubject.Spawn(position, directionSign);
        }

        #endregion
    }
}