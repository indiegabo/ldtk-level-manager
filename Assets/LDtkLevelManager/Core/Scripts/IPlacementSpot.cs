using UnityEngine;

namespace LDtkLevelManager
{
    public interface IPlacementSpot
    {
        string Iid { get; }
        public bool Main { get; }
        Vector2 SpawnPoint { get; }
        int FacingSign { get; }
    }
}