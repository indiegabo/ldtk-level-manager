using System.Collections.Generic;
using LDtkUnity;
using LDtkLevelManager.Utils;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace LDtkLevelManager
{
    /// <summary>
    /// The behaviour of a level created in the LDtk app.
    /// </summary>
    public class LevelBehaviour : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private GameObjectProvider _mainCharacterProvider;

        /// <summary>
        /// Invoked when the player has exited the level.
        /// </summary>
        [SerializeField, Tooltip("Invoked when the player has exited the level.")]
        private UnityEvent<LevelBehaviour> _exitedEvent;

        /// <summary>
        /// Invoked when the level has started its preparation process.
        /// </summary>
        [SerializeField, Tooltip("Invoked when the level has started its preparation process.")]
        private UnityEvent<LevelBehaviour, Vector2> _preparationStartedEvent;

        /// <summary>
        /// Invoked when the level has finished its preparation process.
        /// </summary>
        [SerializeField, Tooltip("Invoked when the level has finished its preparation process.")]
        private UnityEvent<LevelBehaviour, LevelTrail> _preparedEvent;

        /// <summary>
        /// Invoked when the player has entered the level.
        /// </summary>
        [SerializeField, Tooltip("Invoked when the player has entered the level.")]
        private UnityEvent<LevelBehaviour> _enteredEvent;

        #endregion

        #region Fields

        private LDtkIid _ldtkIid;
        private LDtkComponentLevel _ldtkComponentLevel;
        private LevelInfo _info;

        private Dictionary<string, IConnection> _connections;
        private Dictionary<string, IPlacementSpot> _spots;
        private Dictionary<string, IPortal> _portals;

        #endregion

        #region Getters

        /// <summary>
        /// The <see cref="LDtkLevelManager.LevelInfo"/> associated with this <see cref="LevelBehaviour"/>.
        /// </summary>
        public LevelInfo Info => _info;

        /// <summary>
        /// Fired when the player exits the level.
        /// </summary>
        public UnityEvent<LevelBehaviour> PlayerExited => _exitedEvent;

        /// <summary>
        /// Fired when the preparation for the player to enter the level is started.
        /// </summary>
        public UnityEvent<LevelBehaviour, Vector2> PreparationStarted => _preparationStartedEvent;

        /// <summary>
        /// Fired when the level is fully prepared and player is already in position.
        /// Curtains are about to be opened.
        /// </summary>
        public UnityEvent<LevelBehaviour, LevelTrail> Prepared => _preparedEvent;

        /// <summary>
        /// Fired when the player enters the level.
        /// </summary>
        public UnityEvent<LevelBehaviour> PlayerEntered => _enteredEvent;

        #endregion

        #region Behaviour

        private void Awake()
        {
            if (!TryGetComponent(out _ldtkIid))
            {
                Logger.Error($"Level {name} has no {nameof(LDtkIid)} component", this);
                return;
            }

            if (!TryGetComponent(out _ldtkComponentLevel))
            {
                Logger.Error($"Level {name} has no {nameof(LDtkComponentLevel)} component", this);
                return;
            }

            if (!LevelLoader.Instance.TryGetLevel(_ldtkIid.Iid, out _info))
            {
                Logger.Error($"Level {name} could not be activated because {_ldtkIid.Iid} is not present on dictionary", this);
                return;
            }

            name = _info.Name;

            LDtkComponentLayer componentLayer = _ldtkComponentLevel.LayerInstances.FirstOrDefault(
                l => l != null && l.Identifier == LevelLoader.Instance.NavigationLayer
            );

            Transform navigationContainer = componentLayer != null ? componentLayer.transform : transform;

            EvaluateSpots(navigationContainer);
            EvaluateNavigators(navigationContainer);
            EvaluatePortals(navigationContainer);

            LevelLoader.Instance.RegisterAsBehaviour(_ldtkIid.Iid, this);
            BroadcastMessage("OnLevelAwake", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnDestroy()
        {
            LevelLoader.Instance.UnregisterAsBehaviour(_ldtkIid.Iid);
        }

        #endregion

        #region Level Cycle

        /// <summary>
        /// Prepares the level to be entered placing the character at the main spot position and facing the same direction.<br/>
        /// <br/>
        /// If there are no spots in the level, it logs an error, sets the <paramref name="trail"/> to <see cref="LevelTrail.Empty"/> and returns false.<br/>
        /// <br/>
        /// If there is no main spot, it logs a warning, uses the first available spot as main and continues.<br/>
        /// <br/>
        /// </summary>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(out LevelTrail trail)
        {
            if (_spots.Count == 0)
            {
                Logger.Error($"Level {name} could not be prepared because there are no spots", this);
                trail = LevelTrail.Empty;
                return false;
            }

            IPlacementSpot mainSpot = _spots.Values.FirstOrDefault(x => x.Main);

            if (mainSpot == null)
            {
                string message = $"Level {name} is trying to prepare with a main spot but there is no spot set as \"Main\" in the registry.";
                message += $"Using the first available spot.";
                Logger.Warning(message, this);
                mainSpot = _spots.Values.FirstOrDefault();
            }

            // Create a new trail with the level's Iid and the main spot's spawn position and facing sign
            trail = LevelTrail.FromSpot(_info.Iid, mainSpot);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, mainSpot.SpawnPoint);

            // Place the character at the main spot
            PlaceCharacter(mainSpot.SpawnPoint, mainSpot.FacingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, trail);

            return true;
        }

        /// <summary>
        /// Prepares the level by setting the player at the specified spot and sets a 
        /// <see cref="LevelTrail"/> with the level's Iid and the spot's spawn position and facing sign.
        /// </summary>
        /// <param name="spotIid">The Iid of the spot where the player should spawn.</param>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(string spotIid, out LevelTrail trail)
        {
            if (!_spots.TryGetValue(spotIid, out IPlacementSpot registeredSpot))
            {
                string message = $"Level {name} could not be prepared because there is no spot by Iid \"{spotIid}\" present on the spots registry.";
                Logger.Error(message, this);
                trail = LevelTrail.Empty;
                return false;
            }

            // Create a new trail with the level's Iid and the main spot's spawn position and facing sign
            trail = LevelTrail.FromSpot(_info.Iid, registeredSpot);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, registeredSpot.SpawnPoint);

            // Place the character at the main spot
            PlaceCharacter(registeredSpot.SpawnPoint, registeredSpot.FacingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, trail);

            return true;
        }

        /// <summary>
        /// Prepares the level by setting the player at the specified connection and sets a 
        /// <see cref="LevelTrail"/> with the level's Iid and the connection's spawn position 
        /// and facing sign.
        /// </summary>
        /// <param name="connection">The connection to use to enter the level.</param>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(IConnection connection, out LevelTrail trail)
        {
            // It is comming from a connection, so it must find the target connection within the dictionary
            if (!_connections.TryGetValue(connection.TargetIid, out IConnection registeredConnection))
            {
                string message = $"Level {name} could not be prepared because the connection Iid \"{connection.TargetIid}\"";
                message += "is not present on the connections registry.";
                Logger.Error(message, this);
                trail = LevelTrail.Empty;
                return false;
            }

            // Create a new trail with the level's Iid and the connection's spawn position and facing sign
            trail = LevelTrail.FromConnection(_info.Iid, registeredConnection);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, registeredConnection.Spot.SpawnPoint);

            // Place the character at the main spot
            PlaceCharacter(registeredConnection.Spot.SpawnPoint, registeredConnection.Spot.FacingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, trail);

            return true;
        }

        /// <summary>
        /// Prepares the level by setting the player at the specified portal and sets a 
        /// <see cref="LevelTrail"/> with the level's Iid and the portal's spawn position 
        /// and facing sign.
        /// </summary>
        /// <param name="portal">The portal to use to enter the level.</param>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(IPortal portal, out LevelTrail trail)
        {
            // It is comming from a portal, so it must find the target portal within the dictionary
            if (!_portals.TryGetValue(portal.TargetIid, out IPortal registeredPortal))
            {
                // If the portal is not present on the registry, log an error and return false
                string message = $"Level {name} could not be prepared because the portal under Iid \"{portal.TargetIid}\"";
                message += "is not present on the portals registry.";
                Logger.Error(message, this);
                trail = LevelTrail.Empty;
                return false;
            }

            // Create a new trail with the level's Iid and the portal's spawn position and facing sign
            trail = LevelTrail.FromPortal(_info.Iid, registeredPortal);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, registeredPortal.Spot.SpawnPoint);

            // Place the character at the main spot
            PlaceCharacter(registeredPortal.Spot.SpawnPoint, registeredPortal.Spot.FacingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, trail);

            return true;
        }

        /// <summary>
        /// Prepares the level by setting the player at the specified spawn position and sets a 
        /// <see cref="LevelTrail"/> with the level's Iid and the spawn position and facing sign.
        /// </summary>
        /// <param name="spawnPoint">The spawn position of the player.</param>
        /// <param name="facingSign">The direction the player should face.</param>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(Vector2 spawnPoint, int facingSign, out LevelTrail trail)
        {
            // Create a new trail with the level's Iid and the spawn position and facing sign
            trail = LevelTrail.FromPoint(_info.Iid, spawnPoint, facingSign);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, spawnPoint);

            // Place the character at the spawn position
            PlaceCharacter(spawnPoint, facingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, trail);

            // If the level was prepared, return true
            return true;
        }


        /// <summary>
        /// Enters the level enabling all navigators and calling 
        /// <see cref="ICharacterLevelFlowSubject.OnLevelEnter"/> on the main character.
        /// </summary>
        public void Enter()
        {
            // Enable all navigators
            EnableNavigators();

            // Get the main character's level flow subject
            if (!_mainCharacterProvider.TryGetComponent(out ICharacterLevelFlowSubject levelFlowSubject))
            {
                // If it doesn't exist, log an error and return
                Logger.Error($"Level {name} could not find an ({nameof(ICharacterLevelFlowSubject)}) to call {nameof(ICharacterLevelFlowSubject.OnLevelEnter)}", this);
                return;
            }

            // Call OnLevelEnter on the main character's level flow subject
            levelFlowSubject.OnLevelEnter();

            // Broadcast the level entered event
            _enteredEvent.Invoke(this);
        }

        /// <summary>
        /// Exits the level disabling all connections and calling 
        /// <see cref="ICharacterLevelFlowSubject.OnLevelExit"/> on the main character.
        /// </summary>
        public void Exit()
        {
            // Disable all connections
            DisableNavigators();

            if (!_mainCharacterProvider.TryGetComponent(out ICharacterLevelFlowSubject levelFlowSubject))
            {
                // If it doesn't exist, log an error and return
                Logger.Error($"Level {name} could not find an ({nameof(ICharacterLevelFlowSubject)}) to call {nameof(ICharacterLevelFlowSubject.OnLevelExit)}", this);
                return;
            }

            // Call OnLevelExit on the main character's level flow subject
            levelFlowSubject.OnLevelExit();

            // Broadcast the level exited event
            _exitedEvent.Invoke(this);
        }
        #endregion

        #region Navigation

        /// <summary>
        /// Evaluates all <see cref="IPlacementSpot"/> children of the 
        /// <paramref name="navigationContainer"/>
        /// and adds them to the <see cref="_spots"/> dictionary. 
        /// If a spot with the same IID already exists,
        /// a warning is logged and the first found is used.
        /// </summary>
        /// <param name="navigationContainer">The container to look for <see cref="IPlacementSpot"/> components.</param>
        private void EvaluateSpots(Transform navigationContainer)
        {
            _spots = new Dictionary<string, IPlacementSpot>();

            IPlacementSpot[] spotComponents = navigationContainer.GetComponentsInChildren<IPlacementSpot>();

            foreach (IPlacementSpot spot in spotComponents)
            {
                if (_spots.ContainsKey(spot.Iid))
                {
                    Logger.Warning($"Level {name} has more than one spots with the same Iid: {spot.Iid}. Using the first found", this);
                }

                _spots.Add(spot.Iid, spot);
            }
        }

        /// <summary>
        /// Evaluates all <see cref="IConnection"/> children of the 
        /// <paramref name="navigationContainer"/>
        /// and adds them to the <see cref="_connections"/> dictionary. 
        /// If a connection with the same IID already exists,
        /// a warning is logged and the first found is used.
        /// </summary>
        /// <param name="navigationContainer">The container to look for <see cref="IConnection"/> components.</param>
        private void EvaluateNavigators(Transform navigationContainer)
        {
            _connections = new Dictionary<string, IConnection>();

            IConnection[] connectionsComponents = navigationContainer.GetComponentsInChildren<IConnection>();

            foreach (IConnection connection in connectionsComponents)
            {
                if (_connections.ContainsKey(connection.Iid))
                {
                    Logger.Warning($"Level {name} has more than one connection with the same Iid: {connection.Iid}. Using the first found.", this);
                }
                _connections.Add(connection.Iid, connection);
                connection.Initialize();
                connection.SetActive(false);
            }
        }

        /// <summary>
        /// Enable all navigators in the level.
        /// </summary>
        private void EnableNavigators()
        {
            // Enable all connections in the level
            foreach (IConnection connection in _connections.Values)
            {
                connection.SetActive(true);
            }

            // Enable all portals in the level
            foreach (IPortal portal in _portals.Values)
            {
                portal.SetActive(true);
            }
        }

        /// <summary>
        /// Disable all navigators in the level.
        /// </summary>
        private void DisableNavigators()
        {
            // Disable all connections in the level
            foreach (IConnection connection in _connections.Values)
            {
                connection.SetActive(false);
            }

            // Disable all portals in the level
            foreach (IPortal portal in _portals.Values)
            {
                portal.SetActive(false);
            }
        }

        /// <summary>
        /// Evaluates all <see cref="IPortal"/> components that are children of the given <paramref name="navigationContainer"/>.
        /// </summary>
        /// <param name="navigationContainer">The transform that contains all the navigators in the level.</param>
        private void EvaluatePortals(Transform navigationContainer)
        {
            // Create a dictionary to store all the portals in the level
            _portals = new Dictionary<string, IPortal>();

            // Get all the portal components that are children of the navigation container
            IPortal[] portalComponents = navigationContainer.GetComponentsInChildren<IPortal>();

            // Iterate over all the portal components and add them to the dictionary
            foreach (IPortal portal in portalComponents)
            {
                // Check if there is already a portal with the same Iid in the dictionary
                if (_portals.ContainsKey(portal.Iid))
                {
                    // Log a warning if there is already a portal with the same Iid
                    Logger.Warning($"Level {name} has more than one portal with the same Iid: {portal.Iid}. Using the first found.", this);
                }
                // Add the portal to the dictionary
                _portals.Add(portal.Iid, portal);
            }
        }

        #endregion

        #region Main Character

        /// <summary>
        /// Places the main character in the level at the given position with the given direction sign.
        /// </summary>
        /// <param name="position">The position to place the main character in.</param>
        /// <param name="directionSign">The direction sign to place the main character with.</param>
        private void PlaceCharacter(Vector2 position, int directionSign)
        {
            // Try to get the main character's ICharacterLevelFlowSubject component
            if (!_mainCharacterProvider.TryGetComponent(out ICharacterLevelFlowSubject levelFlowSubject))
            {
                // Log an error if the component is not found
                Logger.Error($"Level {name} could not find an ({nameof(ICharacterLevelFlowSubject)}) to place the character into", this);

                // Return to avoid a null reference exception
                return;
            }

            // Call the PlaceInLevel method on the main character's ICharacterLevelFlowSubject
            levelFlowSubject.PlaceInLevel(position, directionSign);
        }

        #endregion
    }
}