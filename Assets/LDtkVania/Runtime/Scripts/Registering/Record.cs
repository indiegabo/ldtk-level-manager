using UnityEngine;

namespace HandyFSM.Registering
{
    [System.Serializable]
    public class Record
    {
        #region Inspector

        [SerializeField]
        private bool _isScriptable;

        [SerializeField]
        private State _fromState;

        [SerializeField]
        private State _state;

        [SerializeField]
        private ScriptableState _fromScriptable;

        [SerializeField]
        private ScriptableState _scriptableState;

        public void DisplayStateName()
        {
            Debug.Log($"{_state.Name}");
        }

        #endregion

        #region Getters

        public bool IsScriptable => _isScriptable;

        public ScriptableState FromScriptable => _fromScriptable;
        public State FromState => _fromState;
        public ScriptableState ScriptableState => _scriptableState;
        public State State => _state;

        #endregion

        #region Consctructors

        public Record(IState from, IState state)
        {
            if (from is State)
            {
                _fromState = from as State;
            }
            else if (from is ScriptableState)
            {
                _fromScriptable = from as ScriptableState;
            }

            if (state is State)
            {
                _state = state as State;
            }
            else if (state is ScriptableState)
            {
                _isScriptable = true;
                _scriptableState = state as ScriptableState;
            }
        }

        #endregion
    }
}