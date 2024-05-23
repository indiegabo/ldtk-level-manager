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
        private Dictionary<string, IPlacementSpot> _spots;
        private Dictionary<string, IPortal> _portals;

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

        public void Prepare()
        {
            if (_spots.Count == 0)
            {
                MV_Logger.Error($"Level {name} could not be prepared because there are no spots", this);
                return;
            }

            IPlacementSpot mainSpot = _spots.Values.FirstOrDefault(x => x.Main);
            if (mainSpot == null)
            {
                string message = $"Level {name} is trying to prepare with a main spot but there is no spot set as \"Main\" in the registry.";
                message += $"Using the first available spot.";
                MV_Logger.Warning(message, this);
                mainSpot = _spots.Values.FirstOrDefault();
            }

            _preparationStartedEvent.Invoke(this, mainSpot.SpawnPoint);
            PlaceCharacter(mainSpot.SpawnPoint, mainSpot.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromPoint(mainSpot.SpawnPoint));
        }

        public void Prepare(string spotIid)
        {
            if (!_spots.TryGetValue(spotIid, out IPlacementSpot registeredSpot))
            {
                MV_Logger.Error($"Level {name} could not be prepared because there is no spot by Iid \"{spotIid}\" present on the spots registry.", this);
                return;
            }

            _preparationStartedEvent.Invoke(this, registeredSpot.SpawnPoint);
            PlaceCharacter(registeredSpot.SpawnPoint, registeredSpot.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromSpot(registeredSpot));
        }

        public void Prepare(IConnection connection)
        {
            // It is comming from a connection, so it must find the target connection within the dictionary
            if (!_connections.TryGetValue(connection.TargetIid, out IConnection registeredConnection))
            {
                string message = $"Level {name} could not be prepared because the connection Iid \"{connection.TargetIid}\"";
                message += "is not present on the connections registry.";
                MV_Logger.Error(message, this);
                return;
            }

            _preparationStartedEvent.Invoke(this, registeredConnection.Spot.SpawnPoint);
            PlaceCharacter(registeredConnection.Spot.SpawnPoint, registeredConnection.Spot.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromConnection(registeredConnection));
        }

        public void Prepare(IPortal portal)
        {
            // It is comming from a portal, so it must find the target portal within the dictionary
            if (!_portals.TryGetValue(portal.TargetIid, out IPortal registeredPortal))
            {
                string message = $"Level {name} could not be prepared because the portal under Iid \"{portal.TargetIid}\"";
                message += "is not present on the portals registry.";
                MV_Logger.Error(message, this);
                return;
            }

            _preparationStartedEvent.Invoke(this, registeredPortal.Spot.SpawnPoint);
            PlaceCharacter(registeredPortal.Spot.SpawnPoint, registeredPortal.Spot.FacingSign);
            _preparedEvent.Invoke(this, MV_LevelTrail.FromPortal(registeredPortal));
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