namespace HandyFSM
{
    /// <summary>
    /// The state machine
    /// </summary>
    public abstract class GenericHandyMachine<TBaseState, TDefaultState> : HandyMachine
    {
        #region Machine Engine

        /// <summary>
        /// This method recognizes and initializes the states for the machine.
        /// </summary>
        protected virtual void BeforeInitialized()
        {
            _stateProvider.LoadStatesFromBaseType(typeof(TBaseState), false);
            _defaultState = _stateProvider.Get(typeof(TDefaultState));
        }

        #endregion
    }

    /// <summary>
    /// The state machine
    /// </summary>
    public abstract class GenericHandyMachine<TBaseState> : HandyMachine
    {
        #region Machine Engine

        /// <summary>
        /// This method recognizes and initializes the states for the machine.
        /// </summary>
        protected virtual void BeforeInitialized()
        {
            _stateProvider.LoadStatesFromBaseType(typeof(TBaseState), false);
        }

        #endregion
    }

}
