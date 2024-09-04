using UnityEngine;
using UnityEngine.Events;

namespace LDtkLevelManager.PlayerNavigation
{
    [CreateAssetMenu(fileName = "Player Control Bridge", menuName = "LDtkLevelManager/Player Control/Bridge")]
    public class PlayerControlBridge : ScriptableObject
    {
        #region Inspector     

        [SerializeField]
        private UnityEvent<bool> _playerControlChanged;

        #endregion

        #region Fields

        private bool _controlled;

        #endregion

        #region Getters

        public bool IsControlled => _controlled;
        public bool IsUncontrolled => !_controlled;

        // Events
        public UnityEvent<bool> PlayerControlChanged => _playerControlChanged;

        #endregion

        #region Requests

        public void GiveControl()
        {
            _playerControlChanged.Invoke(true);
            _controlled = true;
        }

        public void RemoveControl()
        {
            _playerControlChanged.Invoke(false);
            _controlled = false;
        }

        #endregion
    }
}
