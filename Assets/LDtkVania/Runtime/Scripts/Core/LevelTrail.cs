using UnityEngine;

namespace LDtkVania
{
    [System.Serializable]
    public struct LevelTrail
    {
        #region Static

        public static LevelTrail FromPoint(string levelIid, Vector2 point, int facingSign = 1)
        {
            return new LevelTrail
            {
                _levelIid = levelIid,
                _spawnPosition = point,
                _facingSign = facingSign,
            };
        }

        public static LevelTrail FromSpot(string levelIid, IPlacementSpot spot)
        {
            return new LevelTrail
            {
                _levelIid = levelIid,
                _spawnPosition = spot.SpawnPoint,
                _facingSign = spot.FacingSign
            };
        }

        public static LevelTrail FromConnection(string levelIid, IConnection connection)
        {
            return new LevelTrail
            {
                _levelIid = levelIid,
                _spawnPosition = connection.Spot.SpawnPoint,
                _facingSign = connection.Spot.FacingSign
            };
        }

        public static LevelTrail FromPortal(string levelIid, IPortal portal)
        {
            return new LevelTrail
            {
                _levelIid = levelIid,
                _spawnPosition = portal.Spot.SpawnPoint,
                _facingSign = portal.Spot.FacingSign
            };
        }

        public static LevelTrail Empty => new()
        {
            _levelIid = string.Empty,
            _spawnPosition = Vector2.zero,
            _facingSign = 1
        };

        #endregion

        #region Fields

        [SerializeField]
        private string _levelIid;

        [SerializeField]
        private Vector2 _spawnPosition;

        [Range(-1, 1)]
        [SerializeField]
        private int _facingSign;

        #endregion

        #region Properties

        public string LevelIid { readonly get => _levelIid; set => _levelIid = value; }
        public Vector2 SpawnPosition { readonly get => _spawnPosition; set => _spawnPosition = value; }
        public int FacingSign { readonly get => _facingSign; set => _facingSign = value; }

        #endregion

        #region Getters

        /// <summary>
        /// Checks if the trail (<see cref="LevelTrail"/>) is valid. <br />
        /// A trail is valid if the level Iid is not empty.
        /// </summary>
        public readonly bool IsValid => string.IsNullOrEmpty(_levelIid);

        #endregion
    }
}
