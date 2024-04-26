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
                _connectionKey = null,
                _spawnPosition = point,
                _facingSign = facingSign,
            };
        }

        public static MV_LevelTrail FromCheckpoint(ILevelAnchor checkpoint)
        {
            return new MV_LevelTrail
            {
                _connectionKey = null,
                _spawnPosition = checkpoint.SpawnPoint,
                _facingSign = checkpoint.FacingSign
            };
        }

        public static MV_LevelTrail FromConnection(IConnection connection)
        {
            return new MV_LevelTrail
            {
                _connectionKey = connection.Key,
                _spawnPosition = connection.SpawnPoint,
                _facingSign = connection.FacingSign
            };
        }

        #endregion

        #region Fields

        [SerializeField]
        private string _connectionKey;

        [SerializeField]
        private Vector2 _spawnPosition;

        [Range(-1, 1)]
        [SerializeField]
        private int _facingSign;

        #endregion

        #region Properties

        public string ConnectionKey { readonly get => _connectionKey; set => _connectionKey = value; }
        public Vector2 SpawnPosition { readonly get => _spawnPosition; set => _spawnPosition = value; }
        public int FacingSign { readonly get => _facingSign; set => _facingSign = value; }

        #endregion
    }
}
