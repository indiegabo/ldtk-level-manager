using LDtkUnity;
using UnityEngine;

namespace LDtkVania
{
    public class MV_Portal : MonoBehaviour, IPortal
    {
        private LDtkIid _ldtkIid;
        private LDtkFields _fields;
        private MV_PlacementSpot _spot;

        private string _targetLevelIid;
        private string _targetPortalIid;

        #region Behaviour

        private void Awake()
        {
            _ldtkIid = GetComponent<LDtkIid>();
            _fields = GetComponent<LDtkFields>();

            LDtkReferenceToAnEntityInstance entityRef = _fields.GetEntityReference("Target");
            _targetPortalIid = entityRef.EntityIid;
            _targetLevelIid = entityRef.LevelIid;
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

        #endregion
    }
}