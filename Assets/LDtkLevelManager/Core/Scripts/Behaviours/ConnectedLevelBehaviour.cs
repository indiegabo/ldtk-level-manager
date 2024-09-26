using System.Collections.Generic;
using LDtkUnity;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace LDtkLevelManager
{
    /// <summary>
    /// The behaviour of a level that is a inter connected levels universe.
    /// </summary>
    public class ConnectedLevelBehaviour : LevelBehaviour
    {
        #region Inspector

        #endregion

        #region Fields

        protected Dictionary<string, IConnection> _connections;
        protected Dictionary<string, IPortal> _portals;

        #endregion

        #region Getters


        #endregion

        #region Level Behaviour

        protected override void ResolvePlacement(Transform placementContainer)
        {
            EvaluateConnections(placementContainer);
            EvaluatePortals(placementContainer);
        }

        #endregion

        #region Level Preparation

        /// <summary>
        /// Prepares the level by setting the player at the specified connection and sets a 
        /// <see cref="FlowSubjectTrail"/> with the level's Iid and the connection's spawn position 
        /// and facing sign.
        /// </summary>
        /// <param name="connection">The connection to use to enter the level.</param>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(ILevelFlowSubject subject, IConnection connection, out FlowSubjectTrail trail)
        {
            // It is comming from a connection, so it must find the target connection within the dictionary
            if (!_connections.TryGetValue(connection.TargetIid, out IConnection registeredConnection))
            {
                string message = $"Level {name} could not be prepared because the connection Iid \"{connection.TargetIid}\"";
                message += "is not present on the connections registry.";
                Logger.Error(message, this);
                trail = FlowSubjectTrail.Empty;
                return false;
            }

            // Create a new trail with the level's Iid and the connection's spawn position and facing sign
            trail = FlowSubjectTrail.FromConnection(_info.Iid, registeredConnection);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, subject, registeredConnection.Spot.SpawnPoint);

            // Place the character at the main spot
            PlaceSubject(subject, registeredConnection.Spot.SpawnPoint, registeredConnection.Spot.FacingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, subject, trail);

            return true;
        }

        /// <summary>
        /// Prepares the level by setting the player at the specified portal and sets a 
        /// <see cref="FlowSubjectTrail"/> with the level's Iid and the portal's spawn position 
        /// and facing sign.
        /// </summary>
        /// <param name="portal">The portal to use to enter the level.</param>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(ILevelFlowSubject subject, IPortal portal, out FlowSubjectTrail trail)
        {
            // It is comming from a portal, so it must find the target portal within the dictionary
            if (!_portals.TryGetValue(portal.TargetIid, out IPortal registeredPortal))
            {
                // If the portal is not present on the registry, log an error and return false
                string message = $"Level {name} could not be prepared because the portal under Iid \"{portal.TargetIid}\"";
                message += "is not present on the portals registry.";
                Logger.Error(message, this);
                trail = FlowSubjectTrail.Empty;
                return false;
            }

            // Create a new trail with the level's Iid and the portal's spawn position and facing sign
            trail = FlowSubjectTrail.FromPortal(_info.Iid, registeredPortal);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, subject, registeredPortal.Spot.SpawnPoint);

            // Place the character at the main spot
            PlaceSubject(subject, registeredPortal.Spot.SpawnPoint, registeredPortal.Spot.FacingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, subject, trail);

            return true;
        }

        /// <summary>
        /// Prepares the level by setting the player at the specified spawn position and sets a 
        /// <see cref="FlowSubjectTrail"/> with the level's Iid and the spawn position and facing sign.
        /// </summary>
        /// <param name="spawnPoint">The spawn position of the player.</param>
        /// <param name="facingSign">The direction the player should face.</param>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(ILevelFlowSubject subject, Vector2 spawnPoint, int facingSign, out FlowSubjectTrail trail)
        {
            // Create a new trail with the level's Iid and the spawn position and facing sign
            trail = FlowSubjectTrail.FromPoint(_info.Iid, spawnPoint, facingSign);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, subject, spawnPoint);

            // Place the character at the spawn position
            PlaceSubject(subject, new Vector3(spawnPoint.x, spawnPoint.y, transform.position.z), facingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, subject, trail);

            // If the level was prepared, return true
            return true;
        }

        protected override void BeforeActivation()
        {
            // Enable all navigators
            EnableNavigators();
        }

        protected override void AfterDeactivation()
        {
            // Disable all connections
            DisableNavigators();
        }

        #endregion

        #region Placement

        /// <summary>
        /// Evaluates all <see cref="IConnection"/> children of the 
        /// <paramref name="navigationContainer"/>
        /// and adds them to the <see cref="_connections"/> dictionary. 
        /// If a connection with the same IID already exists,
        /// a warning is logged and the first found is used.
        /// </summary>
        /// <param name="navigationContainer">The container to look for <see cref="IConnection"/> components.</param>
        private void EvaluateConnections(Transform navigationContainer)
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
    }
}