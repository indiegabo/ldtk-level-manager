using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager.Implementations.Basic
{
    /// <summary>
    /// A portal that can be used to transition between distant levels.
    /// </summary>
    public class Portal : MonoBehaviour, IPortal
    {

        [SerializeField]
        private string _playerTag;

        [SerializeField]
        private GameObject _hintContainer;

        [SerializeField]
        private GameObject _textContainer;

        private LDtkIid _ldtkIid;
        private LDtkFields _fields;
        private PlacementSpot _spot;

        private string _targetLevelIid;
        private string _targetPortalIid;

        private Player _player;

        #region Behaviour

        private void Awake()
        {
            _ldtkIid = GetComponent<LDtkIid>();
            _fields = GetComponent<LDtkFields>();

            LDtkReferenceToAnEntityInstance entityRef = _fields.GetEntityReference("Target");
            _targetPortalIid = entityRef.EntityIid;
            _targetLevelIid = entityRef.LevelIid;
        }

        private void Update()
        {
            if (_player == null || !Input.GetKeyDown(KeyCode.E)) return;
            LevelTransitioner.Instance.TransitionToPortal(_player, _targetLevelIid, this);
            _player = null;
        }

        #endregion

        #region IConnection

        public string Iid
        {
            get
            {
                if (_ldtkIid == null)
                {
                    _ldtkIid = GetComponent<LDtkIid>();
                }
                return _ldtkIid.Iid;
            }
        }

        string IPortal.TargetIid => _targetPortalIid;

        IPlacementSpot IPortal.Spot
        {
            get
            {
                if (_spot == null)
                {
                    LDtkReferenceToAnEntityInstance spotRef = _fields.GetEntityReference("Spot");
                    _spot = spotRef.GetEntity().GetComponent<PlacementSpot>();
                }
                return _spot;
            }
        }

        void IPortal.SetActive(bool isActive) { }

        #endregion

        #region Collisions

        /// <summary>
        /// Called when the player enters the portal bounds.
        /// </summary>
        /// <param name="other">The player's collider.</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(_playerTag)) return;
            _player = other.GetComponent<Player>();
            _hintContainer.SetActive(true);
            _textContainer.SetActive(false);
        }

        /// <summary>
        /// Called when the player exits the portal bounds.
        /// </summary>
        /// <param name="other">The player's collider.</param>
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(_playerTag)) return;
            _player = null;
            _hintContainer.SetActive(false);
            _textContainer.SetActive(true);
        }

        #endregion
    }
}