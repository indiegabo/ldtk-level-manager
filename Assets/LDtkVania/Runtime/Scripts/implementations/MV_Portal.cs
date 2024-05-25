using LDtkUnity;
using LDtkVania.Transitioning;
using UnityEngine;

namespace LDtkVania
{
    public class MV_Portal : MonoBehaviour, IPortal
    {
        [SerializeField]
        private MV_LevelTransitionBridge _transitionBridge;

        [SerializeField]
        private string _playerTag;

        private LDtkIid _ldtkIid;
        private LDtkFields _fields;
        private MV_PlacementSpot _spot;

        private string _targetLevelIid;
        private string _targetPortalIid;

        private bool _inRange;

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
            if (!_inRange || !Input.GetKeyDown(KeyCode.E)) return;
            _transitionBridge.TransitionToPortal(_targetLevelIid, this);
            _inRange = false;
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
                    _spot = spotRef.GetEntity().GetComponent<MV_PlacementSpot>();
                }
                return _spot;
            }
        }

        void IPortal.SetActive(bool isActive)
        {
            // gameObject.SetActive(isActive);
        }

        #endregion

        #region Collisions

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(_playerTag)) return;
            _inRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(_playerTag)) return;
            _inRange = false;
        }

        #endregion
    }
}