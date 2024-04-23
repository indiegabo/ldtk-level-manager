using UnityEngine;

namespace LDtkVania
{
    [System.Serializable]
    public class MV_LevelTrail
    {
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
