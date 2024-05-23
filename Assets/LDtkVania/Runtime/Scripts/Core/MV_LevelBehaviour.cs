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
        private MV_Level _level;

        private Dictionary<string, IConnection> _connections;
        private Dictionary<string, IPlacementSpot> _spots;
        private Dictionary<string, IPortal> _portals;

        #endregion

        #region Getters

        /// <summary>
        /// The <see cref="MV_Level"/> associated with this <see cref="MV_LevelBehaviour"/>.
        /// </summary>
        public MV_Level Level => _level;

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

            if (!MV_LevelManager.Instance.TryGetLevel(_ldtkIid.Iid, out _level))
            {
                MV_Logger.Error($"Level {name} could not be activated because {_ldtkIid.Iid} is not present on dictionary", this);
                return;
            }

            name = _level.Name;

            LDtkComponentLayer componentLayer = _ldtkComponentLevel.LayerInstances.FirstOrDefault(
                l => l != null && l.Identifier == MV_LevelManager.Instance.NavigationLayer
            );

            Transform navigationContainer = componentLayer != null ? componentLayer.transform : transform;

            EvaluateSpots(navigationContainer);
            EvaluateConnections(navigationContainer);
            EvaluatePortals(navigationContainer);

            MV_LevelManager.Instance.RegisterAsBehaviour(_ldtkIid.Iid, this);
            BroadcastMessage("OnLevelAwake", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnDestroy()
        {
            MV_LevelManager.Instance.UnregisterAsBehaviour(_ldtkIid.Iid);
        }

        #endregion

        #region Level Cycle

        public bool Prepare(out MV_LevelTrail trail)
        {
            if (_spots.Count == 0)
            {
                MV_Logger.Error($"Level {name} could not be prepared because there are no spots", this);
                trail = MV_LevelTrail.Empty;
                return false;
            }

            IPlacementSpot mainSpot = _spots.Values.FirstOrDefault(x => x.Main);

            if (mainSpot == null)
            {
                string message = $"Level {name} is trying to prepare with a main spot but there is no spot set as \"Main\" in the registry.";
                message += $"Using the first available spot.";
                MV_Logger.Warning(message, this);
                mainSpot = _spots.Values.FirstOrDefault();
            }

            trail = MV_LevelTrail.FromSpot(_level.Iid, mainSpot);
            _preparationStartedEvent.Invoke(this, mainSpot.SpawnPoint);
            PlaceCharacter(mainSpot.SpawnPoint, mainSpot.FacingSign);
            _preparedEvent.Invoke(this, trail);

            return true;
        }

        public bool Prepare(string spotIid, out MV_LevelTrail trail)
        {
            if (!_spots.TryGetValue(spotIid, out IPlacementSpot registeredSpot))
            {
                MV_Logger.Error($"Level {name} could not be prepared because there is no spot by Iid \"{spotIid}\" present on the spots registry.", this);
                trail = MV_LevelTrail.Empty;
                return false;
            }

            trail = MV_LevelTrail.FromSpot(_level.Iid, registeredSpot);
            _preparationStartedEvent.Invoke(this, registeredSpot.SpawnPoint);
            PlaceCharacter(registeredSpot.SpawnPoint, registeredSpot.FacingSign);
            _preparedEvent.Invoke(this, trail);
            return true;
        }

        public bool Prepare(IConnection connection, out MV_LevelTrail trail)
        {
            // It is comming from a connection, so it must find the target connection within the dictionary
            if (!_connections.TryGetValue(connection.TargetIid, out IConnection registeredConnection))
            {
                string message = $"Level {name} could not be prepared because the connection Iid \"{connection.TargetIid}\"";
                message += "is not present on the connections registry.";
                MV_Logger.Error(message, this);
                trail = MV_LevelTrail.Empty;
                return false;
            }

            trail = MV_LevelTrail.FromConnection(_level.Iid, registeredConnection);
            _preparationStartedEvent.Invoke(this, registeredConnection.Spot.SpawnPoint);
            PlaceCharacter(registeredConnection.Spot.SpawnPoint, registeredConnection.Spot.FacingSign);
            _preparedEvent.Invoke(this, trail);
            return true;
        }

        public bool Prepare(IPortal portal, out MV_LevelTrail trail)
        {
            // It is comming from a portal, so it must find the target portal within the dictionary
            if (!_portals.TryGetValue(portal.TargetIid, out IPortal registeredPortal))
            {
                string message = $"Level {name} could not be prepared because the portal under Iid \"{portal.TargetIid}\"";
                message += "is not present on the portals registry.";
                MV_Logger.Error(message, this);
                trail = MV_LevelTrail.Empty;
                return false;
            }

            trail = MV_LevelTrail.FromPortal(_level.Iid, registeredPortal);
            _preparationStartedEvent.Invoke(this, registeredPortal.Spot.SpawnPoint);
            PlaceCharacter(registeredPortal.Spot.SpawnPoint, registeredPortal.Spot.FacingSign);
            _preparedEvent.Invoke(this, trail);
            return true;
        }

        public bool Prepare(Vector2 spawnPoint, int facingSign, out MV_LevelTrail trail)
        {
            trail = MV_LevelTrail.FromPoint(_level.Iid, spawnPoint, facingSign);
            _preparationStartedEvent.Invoke(this, spawnPoint);
            PlaceCharacter(spawnPoint, facingSign);
            _preparedEvent.Invoke(this, trail);
            return true;
        }

        public void Enter()
        {
            EnableConnections();

            if (!_mainCharacterProvider.TryGetComponent(out ICharacterLevelFlowSubject levelFlowSubject))
            {
                MV_Logger.Error($"Level {name} could not find an ({nameof(ICharacterLevelFlowSubject)}) to call {nameof(ICharacterLevelFlowSubject.OnLevelEnter)}", this);
                return;
            }

            levelFlowSubject.OnLevelEnter();
            _enteredEvent.Invoke(this);
        }

        public void Exit()
        {
            DisableNavigators();

            if (!_mainCharacterProvider.TryGetComponent(out ICharacterLevelFlowSubject levelFlowSubject))
            {
                MV_Logger.Error($"Level {name} could not find an ({nameof(ICharacterLevelFlowSubject)}) to call {nameof(ICharacterLevelFlowSubject.OnLevelExit)}", this);
                return;
            }

            levelFlowSubject.OnLevelExit();
            _exitedEvent.Invoke(this);
        }

        #endregion

        #region Navigation

        private void EvaluateSpots(Transform navigationContainer)
        {
            _spots = new Dictionary<string, IPlacementSpot>();

            IPlacementSpot[] spotComponents = navigationContainer.GetComponentsInChildren<IPlacementSpot>();

            foreach (IPlacementSpot spot in spotComponents)
            {
                if (_spots.ContainsKey(spot.Iid))
                {
                    MV_Logger.Warning($"Level {name} has more than one spots with the same Iid: {spot.Iid}. Using the first found", this);
                }

                _spots.Add(spot.Iid, spot);
            }
        }

        private void EvaluateConnections(Transform navigationContainer)
        {
            _connections = new Dictionary<string, IConnection>();

            IConnection[] connectionsComponents = navigationContainer.GetComponentsInChildren<IConnection>();

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
            foreach (IConnection connection in _connections.Values)
            {
                connection.SetActive(true);
            }

            foreach (IPortal portal in _portals.Values)
            {
                portal.SetActive(true);
            }
        }

        private void DisableNavigators()
        {
            foreach (IConnection connection in _connections.Values)
            {
                connection.SetActive(false);
            }

            foreach (IPortal portal in _portals.Values)
            {
                portal.SetActive(false);
            }
        }

        private void EvaluatePortals(Transform navigationContainer)
        {
            _portals = new Dictionary<string, IPortal>();

            IPortal[] portalComponents = navigationContainer.GetComponentsInChildren<IPortal>();

            foreach (IPortal portal in portalComponents)
            {
                if (_portals.ContainsKey(portal.Iid))
                {
                    MV_Logger.Warning($"Level {name} has more than one portal with the same Iid: {portal.Iid}. Using the first found.", this);
                }
                _portals.Add(portal.Iid, portal);
            }
        }

        #endregion

        #region Main Character

        private void PlaceCharacter(Vector2 position, int directionSign)
        {
            if (!_mainCharacterProvider.TryGetComponent(out ICharacterLevelFlowSubject levelFlowSubject))
            {
                MV_Logger.Error($"Level {name} could not find an ({nameof(ICharacterLevelFlowSubject)}) to place the character into", this);
                return;
            }

            levelFlowSubject.PlaceInLevel(position, directionSign);
        }

        #endregion
    }
}