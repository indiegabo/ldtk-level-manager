using System.Collections.Generic;
using LDtkUnity;
using LDtkVania.Utils;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

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
        private LDtkComponentLevel _ldtkComponentLevel;
        private MV_Level _mvLevel;

        private Dictionary<string, IConnection> _connections;
        private Dictionary<string, ILevelAnchor> _anchors;

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
                MV_Logger.Error($"Level {name} has no {nameof(LDtkIid)} component", this);
                return;
            }

            if (!TryGetComponent(out _ldtkComponentLevel))
            {
                MV_Logger.Error($"Level {name} has no {nameof(LDtkComponentLevel)} component", this);
                return;
            }

            if (!MV_LevelManager.Instance.TryGetLevel(_ldtkIid.Iid, out _mvLevel))
            {
                MV_Logger.Error($"Level {name} could not be activated because {_ldtkIid.Iid} is not present on dictionary", this);
                return;
            }

            name = _mvLevel.Name;

            EvaluateConnections();
            EvaluateAnchors();

            MV_LevelManager.Instance.RegisterAsBehaviour(_ldtkIid.Iid, this);

            BroadcastMessage("OnLevelAwake", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnDestroy()
        {
            MV_LevelManager.Instance.UnregisterAsBehaviour(_ldtkIid.Iid);
        }

        #endregion

        #region Level Cycle

        public void Prepare()
        {
            if (_anchors.Count == 0)
            {
                MV_Logger.Error($"Level {name} could not be prepared because there are no anchors", this);
                return;
            }

            ILevelAnchor mainAnchor = _anchors.Values.FirstOrDefault(x => x.Main);
            if (mainAnchor == null)
            {
                string message = $"Level {name} is trying to prepare with a main anchor but there is no anchor set as \"Main\" in the registry.";
                message += $"Using the first available anchor.";
                MV_Logger.Warning(message, this);
            }

            mainAnchor = _anchors.Values.FirstOrDefault();

            _preparationStartedEvent.Invoke(this, mainAnchor.SpawnPoint);
            PlaceCharacter(mainAnchor.SpawnPoint, mainAnchor.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromPoint(mainAnchor.SpawnPoint));
        }

        public void Prepare(string anchorIid)
        {
            if (!_anchors.TryGetValue(anchorIid, out ILevelAnchor registeredAnchor))
            {
                MV_Logger.Error($"Level {name} could not be prepared because there is no anchor by Iid \"{anchorIid}\" present on the anchors registry.", this);
                return;
            }

            _preparationStartedEvent.Invoke(this, registeredAnchor.SpawnPoint);
            PlaceCharacter(registeredAnchor.SpawnPoint, registeredAnchor.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromAnchor(registeredAnchor));
        }

        public void Prepare(IConnection connection)
        {
            // It is comming from a connection, so it must find the target connection withing the dictionary
            if (!_connections.TryGetValue(connection.TargetIid, out IConnection registeredConnection))
            {
                string message = $"Level {name} could not be prepared because the connection Iid \"{connection.TargetIid}\"";
                message += "is not present on the connections registry.";
                MV_Logger.Error(message, this);
                return;
            }

            _preparationStartedEvent.Invoke(this, registeredConnection.Anchor.SpawnPoint);
            PlaceCharacter(registeredConnection.Anchor.SpawnPoint, registeredConnection.Anchor.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromConnection(registeredConnection));
        }

        public void Prepare(Vector2 spawnPoint, int facingSign)
        {
            _preparationStartedEvent.Invoke(this, spawnPoint);
            PlaceCharacter(spawnPoint, facingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromPoint(spawnPoint));
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

        private void EvaluateConnections()
        {
            _connections = new Dictionary<string, IConnection>();

            LDtkComponentLayer componentLayer = _ldtkComponentLevel.LayerInstances.FirstOrDefault(l => l.Identifier == MV_LevelManager.Instance.AnchorsLayerName);
            Transform connectionsContainer = componentLayer != null ? componentLayer.transform : transform;

            if (connectionsContainer == null) return;

            IConnection[] connectionsComponents = connectionsContainer.GetComponentsInChildren<IConnection>();

            foreach (IConnection connection in connectionsComponents)
            {
                if (_connections.ContainsKey(connection.Iid))
                {
                    MV_Logger.Warning($"Level {name} has more than one connection with the same Iid: {connection.Iid}. Using the first found.", this);
                }
                _connections.Add(connection.Iid, connection);
                connection.Initialize();
                connection.SetActive(false);
            }
        }

        private void EnableConnections()
        {
            if (_connections == null) return;

            foreach (IConnection connection in _connections.Values)
            {
                connection.SetActive(true);
            }
        }

        private void DisableConnections()
        {
            if (_connections == null) return;

            foreach (IConnection connection in _connections.Values)
            {
                connection.SetActive(false);
            }
        }

        #endregion

        #region Anchors

        private void EvaluateAnchors()
        {
            _anchors = new Dictionary<string, ILevelAnchor>();

            Transform anchorsTransform = transform.Find(MV_LevelManager.Instance.AnchorsLayerName);

            if (anchorsTransform == null) return;

            ILevelAnchor[] anchorComponents = anchorsTransform.GetComponentsInChildren<ILevelAnchor>();

            foreach (ILevelAnchor anchor in anchorComponents)
            {
                if (_anchors.ContainsKey(anchor.Iid))
                {
                    MV_Logger.Warning($"Level {name} has more than one anchors with the same Iid: {anchor.Iid}. Using the first found", this);
                }

                _anchors.Add(anchor.Iid, anchor);
            }
        }

        #endregion

        #region Main Character

        private void PlaceCharacter(Vector2 position, int directionSign)
        {
            if (!_mainCharacterProvider.TryGetComponent(out ILevelPlacementSubject placementSubject))
            {
                MV_Logger.Error($"Level {name} could not find an ({nameof(ILevelPlacementSubject)}) to place the character into", this);
                return;
            }

            placementSubject.PlaceInLevel(position, directionSign);
        }

        #endregion
    }
}