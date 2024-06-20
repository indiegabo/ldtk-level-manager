using LDtkUnity;
using UnityEngine;

namespace LDtkVania
{
    public class PlacementSpot : MonoBehaviour, IPlacementSpot
    {
        private LDtkIid _ldtkIid;
        private LDtkFields _fields;
        private int _facingSign;
        private bool _main;

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
        public Vector2 SpawnPoint => transform.position;
        public int FacingSign => _facingSign;
        public bool Main => _main;

        #region Behaviour

        private void Awake()
        {
            _ldtkIid = GetComponent<LDtkIid>();
            _fields = GetComponent<LDtkFields>();

            _facingSign = _fields.GetInt("FacingSign");
            _main = _fields.GetBool("Main");
        }

        #endregion

        #region Setup

        #endregion
    }
}