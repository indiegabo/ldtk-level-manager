using System.Collections.Generic;
using System.Linq;
using LDtkUnity;
using UnityEngine;
using UnityEngine.Events;

namespace LDtkLevelManager
{
    /// <summary>
    /// The behaviour of a level created in the LDtk app.
    /// </summary>
    public abstract class LevelBehaviour : MonoBehaviour
    {
        #region Inspector

        [SerializeField] protected Project _project;

        /// <summary>
        /// Invoked when the player has exited the level.
        /// </summary>
        [SerializeField, Tooltip("Invoked when the player has exited the level.")]
        protected UnityEvent<LevelBehaviour> _deactivatedEvent;

        /// <summary>
        /// Invoked when the player has entered the level.
        /// </summary>
        [SerializeField, Tooltip("Invoked when the player has entered the level.")]
        protected UnityEvent<LevelBehaviour> _activatedEvent;

        /// <summary>
        /// Invoked when the level has started its preparation process.
        /// </summary>
        [SerializeField, Tooltip("Invoked when the level has started its preparation process.")]
        protected UnityEvent<LevelBehaviour, ILevelFlowSubject, Vector2> _preparationStartedEvent;

        /// <summary>
        /// Invoked when the level has finished its preparation process.
        /// </summary>
        [SerializeField, Tooltip("Invoked when the level has finished its preparation process.")]
        protected UnityEvent<LevelBehaviour, ILevelFlowSubject, FlowSubjectTrail> _preparedEvent;

        #endregion

        #region Fields

        protected LDtkIid _ldtkIid;
        protected LDtkComponentLevel _ldtkComponentLevel;
        protected LevelInfo _info;
        protected Transform _placementContainer;
        protected LevelLoader _loader;

        protected Dictionary<string, IPlacementSpot> _spots;
        protected List<ILevelFlowSubject> _subjects;

        #endregion


        #region Getters

        /// <summary>
        /// The <see cref="LDtkLevelManager.LevelInfo"/> associated with this <see cref="ConnectedLevelBehaviour"/>.
        /// </summary>
        public LevelInfo Info => _info;

        /// <summary>
        /// Fired when the player exits the level.
        /// </summary>
        public UnityEvent<LevelBehaviour> Deactivated => _deactivatedEvent;

        /// <summary>
        /// Fired when the player enters the level.
        /// </summary>
        public UnityEvent<LevelBehaviour> Activated => _activatedEvent;

        /// <summary>
        /// Fired when the preparation for the player to enter the level is started.
        /// </summary>
        public UnityEvent<LevelBehaviour, ILevelFlowSubject, Vector2> PreparationStarted => _preparationStartedEvent;

        /// <summary>
        /// Fired when the level is fully prepared and player is already in position.
        /// Curtains are about to be opened.
        /// </summary>
        public UnityEvent<LevelBehaviour, ILevelFlowSubject, FlowSubjectTrail> Prepared => _preparedEvent;

        #endregion

        #region Abstractions

        protected abstract void ResolvePlacement(Transform placementContainer);

        #endregion

        #region Behaviour

        protected virtual void Awake()
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

            _loader = LevelLoader.For(_project);

            if (!_loader.TryGetLevel(_ldtkIid.Iid, out _info))
            {
                Logger.Error($"Level {name} could not be activated because {_ldtkIid.Iid} is not present on dictionary", this);
                return;
            }

            name = _info.Name;
            _subjects = new();

            LDtkComponentLayer componentLayer = _ldtkComponentLevel.LayerInstances.FirstOrDefault(
                l => l != null && l.Identifier == _project.NavigationLayer
            );

            _placementContainer = componentLayer != null ? componentLayer.transform : transform;

            EvaluateSpots(_placementContainer);
            ResolvePlacement(_placementContainer);

            _loader.RegisterAsBehaviour(_ldtkIid.Iid, this);

            BroadcastMessage("OnLevelAwake", this, SendMessageOptions.DontRequireReceiver);
        }

        protected virtual void OnDestroy()
        {
            _loader.UnregisterAsBehaviour(_ldtkIid.Iid);
        }

        #endregion

        #region Placement

        /// <summary>
        /// Evaluates all <see cref="IPlacementSpot"/> children of the 
        /// <paramref name="placementeContainer"/>
        /// and adds them to the <see cref="_spots"/> dictionary. 
        /// If a spot with the same IID already exists,
        /// a warning is logged and the first found is used.
        /// </summary>
        /// <param name="placementeContainer">The container to look for <see cref="IPlacementSpot"/> components.</param>
        protected virtual void EvaluateSpots(Transform placementeContainer)
        {
            _spots = new Dictionary<string, IPlacementSpot>();

            IPlacementSpot[] spotComponents = placementeContainer.GetComponentsInChildren<IPlacementSpot>();

            foreach (IPlacementSpot spot in spotComponents)
            {
                if (_spots.ContainsKey(spot.Iid))
                {
                    Logger.Warning($"Level {name} has more than one spots with the same Iid: {spot.Iid}. Using the first found", this);
                }

                _spots.Add(spot.Iid, spot);
            }
        }

        #endregion


        #region Level Preparation

        /// <summary>
        /// Prepares the level to be entered placing the character at the main spot position and facing the same direction.<br/>
        /// <br/>
        /// If there are no spots in the level, it logs an error, sets the <paramref name="trail"/> to <see cref="FlowSubjectTrail.Empty"/> and returns false.<br/>
        /// <br/>
        /// If there is no main spot, it logs a warning, uses the first available spot as main and continues.<br/>
        /// <br/>
        /// </summary>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(ILevelFlowSubject subject, out FlowSubjectTrail trail)
        {
            if (_spots.Count == 0)
            {
                Logger.Error($"Level {name} could not be prepared because there are no spots", this);
                trail = FlowSubjectTrail.Empty;
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
            trail = FlowSubjectTrail.FromSpot(_info.Iid, mainSpot);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, subject, mainSpot.SpawnPoint);

            // Place the character at the main spot
            PlaceSubject(subject, mainSpot.SpawnPoint, mainSpot.FacingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, subject, trail);

            return true;
        }

        /// <summary>
        /// Prepares the level by setting the player at the specified spot and sets a 
        /// <see cref="FlowSubjectTrail"/> with the level's Iid and the spot's spawn position and facing sign.
        /// </summary>
        /// <param name="spotIid">The Iid of the spot where the player should spawn.</param>
        /// <param name="trail">The trail to be used when entering the level.</param>
        /// <returns>True if the level was prepared, false otherwise.</returns>
        public bool Prepare(ILevelFlowSubject subject, string spotIid, out FlowSubjectTrail trail)
        {
            if (!_spots.TryGetValue(spotIid, out IPlacementSpot registeredSpot))
            {
                string message = $"Level {name} could not be prepared because there is no spot by Iid \"{spotIid}\" present on the spots registry.";
                Logger.Error(message, this);
                trail = FlowSubjectTrail.Empty;
                return false;
            }

            // Create a new trail with the level's Iid and the main spot's spawn position and facing sign
            trail = FlowSubjectTrail.FromSpot(_info.Iid, registeredSpot);

            // Broadcast the preparation started event
            _preparationStartedEvent.Invoke(this, subject, registeredSpot.SpawnPoint);

            // Place the character at the main spot
            PlaceSubject(subject, registeredSpot.SpawnPoint, registeredSpot.FacingSign);

            // Broadcast the preparation finished event
            _preparedEvent.Invoke(this, subject, trail);

            return true;
        }

        #endregion

        #region  Activation

        /// <summary>
        /// Enters the level enabling all navigators and calling 
        /// <see cref="ILevelFlowSubject.EnterLevel"/> on the main character.
        /// </summary>
        public virtual void Activate()
        {
            BeforeActivation();

            // Call OnLevelEnter on the main character's level flow subject
            foreach (ILevelFlowSubject levelFlowSubject in _subjects)
            {
                levelFlowSubject.EnterLevel(this);
            }

            AfterActivation();

            // Broadcast the level entered event
            _activatedEvent.Invoke(this);
        }

        /// <summary>
        /// Deactivates the level disabling all connections and calling 
        /// <see cref="ILevelFlowSubject.LeaveLevel"/> on the current subjects.
        /// </summary>
        public void Deactivate()
        {
            BeforeDeactivation();

            // Call OnLevelExit on the main character's level flow subject
            foreach (ILevelFlowSubject levelFlowSubject in _subjects)
            {
                levelFlowSubject.LeaveLevel(this);
            }

            _subjects.Clear();

            AfterDeactivation();

            // Broadcast the level exited event
            _deactivatedEvent.Invoke(this);
        }

        protected virtual void BeforeActivation() { }
        protected virtual void AfterActivation() { }
        protected virtual void BeforeDeactivation() { }
        protected virtual void AfterDeactivation() { }

        #endregion


        #region ILevelFlowSubject

        /// <summary>
        /// Places the main character in the level at the given position with the given direction sign.
        /// </summary>
        /// <param name="position">The position to place the main character in.</param>
        /// <param name="directionSign">The direction sign to place the main character with.</param>
        protected virtual void PlaceSubject(ILevelFlowSubject subject, Vector2 position, int directionSign)
        {
            Vector3 placementPos = new(
                position.x,
                position.y,
                transform.position.z
            );

            _subjects.Add(subject);

            // Call the PlaceInLevel method on the main character's ICharacterLevelFlowSubject
            subject.PlaceInLevel(this, placementPos, directionSign);
        }

        #endregion
    }
}