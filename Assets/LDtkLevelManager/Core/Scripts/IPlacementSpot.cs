using UnityEngine;

namespace LDtkLevelManager
{
    /// <summary>
    /// Represents a placement spot in a level.
    /// </summary>
    public interface IPlacementSpot
    {
        /// <summary>
        /// The LDtk unique identifier of the spot.
        /// </summary>
        string Iid { get; }

        /// <summary>
        /// If the spot is the main spot in the level. 
        /// If true, the player will be placed here when entering the level without specifying a spot.
        /// 
        /// </summary>
        bool Main { get; }

        /// <summary>
        /// The position where the player should spawn when entering the level at this spot.
        /// </summary>
        Vector2 SpawnPoint { get; }

        /// <summary>
        /// The direction sign that the player should have when spawning at this spot. 1 means right, -1 means left.
        /// </summary>
        int FacingSign { get; }
    }

}