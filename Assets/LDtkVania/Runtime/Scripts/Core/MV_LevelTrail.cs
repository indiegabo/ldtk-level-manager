using UnityEngine;

namespace LDtkVania
{
    [System.Serializable]
    public struct MV_LevelTrail
    {
        #region Static

        public static MV_LevelTrail FromPoint(Vector2 point, int facingSign = 1)
        {
            return new MV_LevelTrail
            {
                _iid = null,
                _spawnPosition = point,
                _facingSign = facingSign,
            };
        }

        public static MV_LevelTrail FromAnchor(ILevelAnchor anchor)
        {
            return new MV_LevelTrail
            {
                _iid = anchor.Iid,
                _spawnPosition = anchor.SpawnPoint,
                _facingSign = anchor.FacingSign
            };
        }

        public static MV_LevelTrail FromConnection(IConnection connection)
        {
            return new MV_LevelTrail
            {
                _iid = connection.Iid,
                _spawnPosition = connection.Anchor.SpawnPoint,
                _facingSign = connection.Anchor.FacingSign
            };
        }

        #endregion

        #region Fields

        [SerializeField]
        private string _iid;

        [SerializeField]
        private Vector2 _spawnPosition;

        [Range(-1, 1)]
        [SerializeField]
        private int _facingSign;

        #endregion

        #region Properties

        public string Iid { readonly get => _iid; set => _iid = value; }
        public Vector2 SpawnPosition { readonly get => _spawnPosition; set => _spawnPosition = value; }
        public int FacingSign { readonly get => _facingSign; set => _facingSign = value; }

        #endregion
    }
}
