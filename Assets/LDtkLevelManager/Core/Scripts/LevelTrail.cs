using UnityEngine;

namespace LDtkLevelManager
{
    /// <summary>
    /// Represents a mark on the player's navigation through the Universe.
    /// </summary>
    [System.Serializable]
    public struct LevelTrail
    {
        #region Static

        /// <summary>
        /// Creates a <see cref="LevelTrail"/> from a level Iid, spawn position and optional facing sign.
        /// </summary>
        /// <param name="levelIid">The LDtk unique identifier of the level.</param>
        /// <param name="point">The spawn position of the player.</param>
        /// <param name="facingSign">The direction the player should face.</param>
        /// <returns>A new <see cref="LevelTrail"/> instance.</returns>
        public static LevelTrail FromPoint(string levelIid, Vector2 point, int facingSign = 1)
        {
            return new LevelTrail
            {
                _levelIid = levelIid,
                _spawnPosition = point,
                _facingSign = facingSign,
            };
        }

        /// <summary>
        /// Creates a <see cref="LevelTrail"/> from a level Iid and a <see cref="IPlacementSpot"/>.
        /// </summary>
        /// <param name="levelIid">The LDtk unique identifier of the level.</param>
        /// <param name="spot">The placement spot of the player.</param>
        /// <returns>A new <see cref="LevelTrail"/> instance.</returns>
        public static LevelTrail FromSpot(string levelIid, IPlacementSpot spot)
        {
            return new LevelTrail
            {
                _levelIid = levelIid,
                _spawnPosition = spot.SpawnPoint,
                _facingSign = spot.FacingSign
            };
        }

        /// <summary>
        /// Creates a <see cref="LevelTrail"/> from a level Iid and a <see cref="IConnection"/>.
        /// </summary>
        /// <param name="levelIid">The LDtk unique identifier of the level.</param>
        /// <param name="connection">The connection of the player.</param>
        /// <returns>A new <see cref="LevelTrail"/> instance.</returns>
        public static LevelTrail FromConnection(string levelIid, IConnection connection)
        {
            return new LevelTrail
            {
                _levelIid = levelIid,
                _spawnPosition = connection.Spot.SpawnPoint,
                _facingSign = connection.Spot.FacingSign
            };
        }

        /// <summary>
        /// Creates a <see cref="LevelTrail"/> from a level Iid and a <see cref="IPortal"/>.
        /// </summary>
        /// <param name="levelIid">The LDtk unique identifier of the level.</param>
        /// <param name="portal">The portal of the player.</param>
        /// <returns>A new <see cref="LevelTrail"/> instance.</returns>
        public static LevelTrail FromPortal(string levelIid, IPortal portal)
        {
            return new LevelTrail
            {
                _levelIid = levelIid,
                _spawnPosition = portal.Spot.SpawnPoint,
                _facingSign = portal.Spot.FacingSign
            };
        }

        /// <summary>
        /// An empty <see cref="LevelTrail"/>.
        /// </summary>
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

        /// <summary>
        /// The LDtk unique identifier of the level.
        /// </summary>
        public string LevelIid
        {
            readonly get => _levelIid;
            set => _levelIid = value;
        }

        /// <summary>
        /// The spawn position of the player in the level.
        /// </summary>
        public Vector2 SpawnPosition
        {
            readonly get => _spawnPosition;
            set => _spawnPosition = value;
        }

        /// <summary>
        /// The direction the player should face when entering the level.
        /// </summary>
        public int FacingSign
        {
            readonly get => _facingSign;
            set => _facingSign = value;
        }

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
