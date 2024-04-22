using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace HandyFSM
{
    /// <summary>
    /// The state machine base class
    /// </summary>
    [AddComponentMenu("HandyFSM/State Machine")]
    public class HandyMachine : MonoBehaviour
    {
#if UNITY_EDITOR
        // [ContextMenu("Open Visualizer")]
        // void DoSomething()
        // {
        //     var window = MachineStateVisualizerWindow.OpenEditorWindow();
        //     window.MachineSelectorField.value = this;
        // }
#endif

        #region Inspector

        [SerializeField]
        protected RuntimeInfo _info;

        [SerializeField]
        protected Configuration _config;

        #endregion

        #region Fields        

        protected IState _defaultState;

        protected IState _currentState;
        protected IState _previousState;

        protected bool _isInitialized;
        protected StateProvider _stateProvider;

        protected Dictionary<string, UnityEvent> _triggers;

        #endregion

        #region Getters

        /// <summary>
        /// The state machine Owner trandform. If not defined on inspector it will be the Transform
        /// of the GameObject in which the script is attached
        /// </summary>
        public Transform Owner => _config.Owner != null ? _config.Owner : transform;

        /// <summary>
        /// If the machine is already
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// If the machine is on
        /// </summary>
        public bool IsOn => _info.Status.Equals(MachineStatus.On);

        /// <summary>
        /// If the machine is paused
        /// </summary>
        public bool IsPaused => _info.Status.Equals(MachineStatus.Paused);

        /// <summary>
        /// If the machine is off
        /// </summary>
        public bool IsOff => _info.Status.Equals(MachineStatus.Off);

        /// <summary>
        /// If the machine is working. Either On or Paused
        /// </summary>
        public bool IsWorking => IsOn || IsPaused;

        /// <summary>
        /// A getter for the machine's Status
        /// </summary>
        public MachineStatus Status => _info.Status;

        /// <summary>
        /// This is the current active state for the this State Machine
        /// </summary>
        public IState CurrentState => _currentState;

        /// <summary>
        /// This is the immediate previous state the machine was in.
        /// </summary>
        public IState PreviousState => _previousState;

        /// <summary>
        /// Getter for the machine's default state
        /// </summary>
        public IState DefaultState => _defaultState;

        /// <summary>
        /// If CurrentStateName should be shown in the inspector
        /// </summary>
        protected bool ShowCurrentState => !Status.Equals(MachineStatus.Off);

        // Triggers
        public Dictionary<string, UnityEvent> Triggers => _triggers;

        // Events

        /// <summary>
        /// Whenever the machine status changes
        /// </summary>
        public UnityEvent<MachineStatus> StatusChanged => _config.StatusChanged;

        /// <summary>
        /// Whenever the current state changes
        /// </summary>
        public UnityEvent<IState, IState> StateChanged => _config.StateChanged;

        #endregion

        #region Behaviour   

        protected virtual void Awake()
        {
            _info.Status = MachineStatus.Off;

            _stateProvider = new StateProvider(this);
            _stateProvider.LoadStatesFromScriptablesList(_config.ScriptableStates, false);

            _triggers = new Dictionary<string, UnityEvent>();
            _config.TriggerItems.ForEach(x => _triggers.Add(x.Key, x.Trigger));

            Type machineType = GetType();

            MethodInfo beforeInitializeMethod = machineType.GetMethod("BeforeInitialized", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            beforeInitializeMethod?.Invoke(this, null);

            _stateProvider.InitializeAllStates();

            if (_config.DefaultScriptableState != null)
                _defaultState = _stateProvider.Get(_config.DefaultScriptableState.GetType());

            _isInitialized = true;

            MethodInfo afterInitializedMethod = machineType.GetMethod("AfterInitialized", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            afterInitializedMethod?.Invoke(this, null);
        }

        protected virtual void Start()
        {
            if (!_config.InitalizationMode.Equals(InitializationMode.Automatic)) return;

            if (_defaultState == null)
            {
                Debug.LogError($"The machine {name} is marked to initialize automatically but was unable to resolve a default state.", this);
                return;
            }

            TurnOn(_defaultState);
        }

        protected virtual void Update()
        {
            if (!_info.Status.Equals(MachineStatus.On)) return;

            EvaluateTransition();
            _currentState?.Tick();
        }

        protected virtual void LateUpdate()
        {
            if (!_info.Status.Equals(MachineStatus.On)) return;

            EvaluateTransition();
            _currentState?.LateTick();
        }

        protected virtual void FixedUpdate()
        {
            if (!_info.Status.Equals(MachineStatus.On)) return;

            EvaluateTransition();
            _currentState?.FixedTick();
        }

        protected virtual void OnDisable()
        {
            Stop();
        }

        #endregion

        #region Machine Engine

        /// <summary>
        /// Turns the machine on and enters the given state
        /// </summary>
        /// <param name="stateType"></param>
        public virtual void TurnOn(Type stateType)
        {
            if (!_stateProvider.TryGet(stateType, out IState state))
            {
                Debug.LogError($"Trying to turn machine on but {stateType.Name} is not loaded.", this);
                return;
            }

            TurnOn(state);
        }

        /// <summary>
        /// Turns the machine on and enters the given state
        /// </summary>
        /// <param name="state"></param>
        public virtual void TurnOn(IState state)
        {
            if (IsWorking)
            {
                Debug.LogError($"Trying to turn machine on but it is already working", this);
                return;
            }

            if (!_stateProvider.IsLoaded(state))
            {
                Debug.LogError($"Trying to turn machine on but {nameof(state)} is not loaded.", this);
                return;
            }

            ChangeStatus(MachineStatus.On);
            ChangeState(state);
        }

        /// <summary>
        /// Pauses the machine
        /// </summary>
        public virtual void Resume()
        {
            if (!IsPaused) return;
            ChangeStatus(MachineStatus.On);
        }

        /// <summary>
        /// Pauses the machine
        /// </summary>
        public virtual void Pause()
        {
            if (!IsOn) return;
            ChangeStatus(MachineStatus.Paused);
        }

        /// <summary>
        /// Stops the machine
        /// </summary>
        public virtual void Stop()
        {
            if (!IsWorking) return;

            _currentState?.Exit();
            _currentState = null;

            ChangeStatus(MachineStatus.Off);
        }

        /// <summary>
        /// Changes the status of the machine
        /// </summary>
        /// <param name="status"></param>
        public virtual void ChangeStatus(MachineStatus status)
        {
            _info.Status = status;
            _config.StatusChanged?.Invoke(_info.Status);

            if (status.Equals(MachineStatus.Off))
            {
                _info.CurrentStateName = "None";
            }
        }

        #endregion

        #region Providing The machine

        /// <summary>
        /// Casts the current instance to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <returns>The instance casted to the specified type.</returns>
        public T As<T>() where T : HandyMachine
        {
            return this as T;
        }

        #endregion

        #region Machine's Logic

        /// <summary>
        /// Defines a given state as active
        /// </summary>
        /// <param name="state"> The state to be set as active </param>
        /// <param name="forceInterruption"> If an uninterruptible state should be interrupted </param>
        public virtual void RequestStateChange(IState state, StateChangeMode mode = StateChangeMode.Respectfully)
        {
            if (_info.Status != MachineStatus.On || state == null) return;

            if (_currentState != null && !_currentState.Interruptible && mode.Equals(StateChangeMode.Respectfully)) return; // State cannot be interrupted, but will if forced.

            ChangeState(state);
        }

        /// <summary>
        /// Defines a given state as active
        /// </summary>
        /// <param name="state"> The state to be set as active </param>
        /// <param name="forceInterruption"> If an uninterruptible state should be interrupted </param>
        public virtual void RequestStateChange<T>(StateChangeMode mode = StateChangeMode.Respectfully) where T : State
        {
            if (!_stateProvider.TryGet<T>(out IState state))
            {
                Debug.LogError($"A state under the Type {nameof(T)} was requested but it is not present int the state factory ", this);
                return;
            }

            RequestStateChange(state, mode);
        }

        /// <summary>
        /// Ends the current state of the machine.
        /// </summary>
        /// <param name="target">The target state to change to. If null, the default state will be used.</param>
        public virtual void EndState(IState target = null)
        {
            // Check if the machine is turned on
            if (_info.Status != MachineStatus.On)
            {
                Debug.LogError($"Trying to end state on a machine that is not turned on. ", this);
                return;
            }

            // Change to the target state if provided
            if (target != null)
            {
                ChangeState(target);
                return;
            }

            // Change to the default state if available
            if (_defaultState != null)
            {
                ChangeState(_defaultState);
                return;
            }

            // Invoke the exit action of the current state
            _currentState?.Exit();
        }

        /// <summary>
        /// Ends the specified state of type T.
        /// </summary>
        /// <typeparam name="T">The type of the state.</typeparam>
        public virtual void EndState<T>() where T : IState
        {
            // Check if the requested state of type T exists in the state factory
            if (!_stateProvider.TryGet<T>(out IState state))
            {
                Debug.LogError($"A state under the Type {nameof(T)} was requested but it is not present in the state factory ", this);
                return;
            }

            // End the specified state
            EndState(state);
        }

        /// <summary>
        /// Changes the state.
        /// </summary>
        /// <param name="state">The new state to change to.</param>
        protected virtual void ChangeState(IState state)
        {
            // Do not change state if it is the same as the current state or null
            if (state == _currentState || state == null) return;

            // Define the previous state
            _previousState = _currentState;

            // Invoke the exit action of the current state
            _currentState?.Exit();

            // Change the current state
            _currentState = state;

            // Announce the new state
            _config.StateChanged.Invoke(_currentState, _previousState);

            // Invoke the enter action of the new state
            _currentState.Enter();

            // Update the current state name
            _info.CurrentStateName = CurrentState.Name;
        }

        /// <summary>
        /// Handles the tick event.
        /// </summary>
        protected virtual void EvaluateTransition()
        {
            if (_currentState == null) return;

            // Evaluate the next state
            if (_currentState.ShouldTransition(out IState targetState))
            {
                RequestStateChange(targetState);
            }
        }

        #endregion

        #region Providing States

        /// <summary>
        /// Load states from the specified state type.
        /// </summary>
        /// <param name="stateType">The type of the states to load.</param>
        public void LoadStatesOfType(Type stateType)
        {
            _stateProvider.LoadStatesFromBaseType(stateType);
        }

        /// <summary>
        /// Loads and initalizes states from a list of ScriptableState objects
        /// </summary>
        /// <param name="states">A list of ScriptableState objects to load.</param>
        public void LoadStatesFromScriptablesList(List<ScriptableState> states)
        {
            _stateProvider.LoadStatesFromScriptablesList(states, true);
        }
        /// <summary>
        /// Loads the state of the given type.
        /// </summary>
        /// <param name="stateType">The type of the state to load.</param>
        public void LoadState(Type stateType)
        {
            _stateProvider.LoadState(stateType);
        }

        /// <summary>
        /// Loads the state by passing it to the state provider.
        /// </summary>
        /// <param name="state">The state object to be loaded.</param>
        public void LoadState(IState state)
        {
            _stateProvider.LoadState(state);
        }

        /// <summary>
        /// Retrieves the state of the specified type.
        /// </summary>
        /// <param name="stateType">The type of the state to retrieve.</param>
        /// <returns>The state of the specified type.</returns>
        public IState GetState(Type stateType)
        {
            // Use the _stateProvider to get the state of the specified type.
            return _stateProvider.Get(stateType);
        }

        /// <summary>
        /// Tries to get the state of the specified type.
        /// </summary>
        /// <param name="stateType">The type of the state to get.</param>
        /// <param name="state">When this method returns, contains the state associated with the specified type, if found; otherwise, null.</param>
        /// <returns><c>true</c> if the state of the specified type was found; otherwise, <c>false</c>.</returns>
        public bool TryGetState(Type stateType, out IState state)
        {
            return _stateProvider.TryGet(stateType, out state);
        }

        /// <summary>
        /// Retrieves the state of type T loaded into this machine.
        /// </summary>
        /// <typeparam name="T">The type of state to retrieve.</typeparam>
        /// <returns>The state of type T.</returns>
        public T GetState<T>() where T : IState
        {
            // Use the _stateProvider to get the state of type T
            return _stateProvider.Get<T>();
        }

        /// <summary>
        /// Tries to get the state of type T loaded into this machine.
        /// </summary>
        /// <typeparam name="T">The type of the state to get.</typeparam>
        /// <param name="state">The retrieved state of type T.</param>
        /// <returns>True if the state was successfully retrieved, false otherwise.</returns>
        public bool TryGetState<T>(out IState state) where T : IState
        {
            return _stateProvider.TryGet<T>(out state);
        }

        /// <summary>
        /// Gets all states loaded into this machine. 
        /// This can have an expensive performance since
        /// it iterates all states in the state provider in order to 
        /// form a list. Never use this method in a loop.
        /// </summary>
        /// <returns>A list of IState objects.</returns>
        public List<IState> GetAllStates()
        {
            return _stateProvider.GetAllStates();
        }

        #endregion

        #region Triggers

        public void SqueezeTrigger(string key)
        {
            if (!_triggers.ContainsKey(key)) return;
            _triggers[key].Invoke();
        }

        public void RegisterOnTrigger(string key, UnityAction action)
        {
            if (!_triggers.ContainsKey(key))
            {
                Debug.LogError($"Trigger '{key}' does not exist. For StateMachine {gameObject.name}");
                return;
            }

            _triggers[key].AddListener(action);
        }

        public void UnregisterOnTrigger(string key, UnityAction action)
        {
            if (!_triggers.ContainsKey(key))
            {
                Debug.LogError($"Trigger '{key}' does not exist. For StateMachine {gameObject.name}");
                return;
            }

            _triggers[key].RemoveListener(action);
        }

        #endregion
    }

}
