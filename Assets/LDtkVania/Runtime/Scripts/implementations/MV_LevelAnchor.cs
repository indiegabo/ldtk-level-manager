using LDtkUnity;
using UnityEngine;

namespace LDtkVania
{
    public class MV_LevelAnchor : MonoBehaviour, ILevelAnchor
    {
        private LDtkIid _ldtkIid;
        private LDtkFields _fields;
        private int _facingSign;

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

        #region Behaviour

        private void Awake()
        {
            _ldtkIid = GetComponent<LDtkIid>();
            _fields = GetComponent<LDtkFields>();

            _facingSign = _fields.GetInt("FacingSign");
        }

        #endregion


        #region Setup

        #endregion
    }
}