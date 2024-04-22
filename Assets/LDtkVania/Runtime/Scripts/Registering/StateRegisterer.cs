using UnityEngine;

namespace HandyFSM.Registering
{
    [RequireComponent(typeof(HandyMachine))]
    public class StateRegisterer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private StateRegistry _registry;

        #endregion

        #region Fields

        private HandyMachine _machine;
        private float _sessionDuration;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _machine = GetComponent<HandyMachine>();
        }

        private void Start()
        {
            _sessionDuration = 0f;
        }

        private void Update()
        {
            _sessionDuration += Time.deltaTime;
        }

        private void OnEnable()
        {
            _registry.OpenSession(_machine);
            _registry.Register(_machine.CurrentState);
            _machine.StateChanged.AddListener(OnStateChanged);
        }

        private void OnDisable()
        {
            _machine.StateChanged.RemoveListener(OnStateChanged);
            _registry.CloseSession(_sessionDuration);
        }

        #endregion

        #region Callbacks

        private void OnStateChanged(IState newState, IState previous)
        {
            _registry.Register(newState);
        }

        #endregion
    }
}