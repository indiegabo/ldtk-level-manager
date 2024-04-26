using UnityEngine;

namespace LDtkVania
{
    public interface ILevelAnchor
    {
        string AnchorIid { get; }
        string LevelIId { get; }
        Vector2 SpawnPoint { get; }
        int FacingSign { get; }
    }
}