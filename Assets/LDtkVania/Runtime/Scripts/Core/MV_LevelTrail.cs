using UnityEngine;

namespace LDtkVania
{
    [System.Serializable]
    public struct MV_LevelTrail
    {
        #region Static

        public static MV_LevelTrail FromPoint(Vector2 point)
        {
            return new MV_LevelTrail
            {
                _connectionKey = null,
                _spawnPosition = point,
                _directionSign = 0
            };
        }

        public static MV_LevelTrail FromCheckpoint(MV_ICheckpoint checkpoint)
        {
            return new MV_LevelTrail
            {
                _connectionKey = null,
                _spawnPosition = checkpoint.SpawnPosition,
                _directionSign = checkpoint.DirectionSign
            };
        }

        public static MV_LevelTrail FromConnection(MV_IConnection connection)
        {
            return new MV_LevelTrail
            {
                _connectionKey = connection.Key,
                _spawnPosition = connection.SpawnPosition,
                _directionSign = connection.DirectionSign
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
        private int _directionSign;

        #endregion

        #region Properties

        public string ConnectionKey { get => _connectionKey; set => _connectionKey = value; }
        public Vector2 SpawnPosition { get => _spawnPosition; set => _spawnPosition = value; }
        public int DirectionSign { get => _directionSign; set => _directionSign = value; }

        #endregion
    }
}
