using UnityEngine;

namespace LDtkVania
{
    public interface MV_ICheckpoint
    {
        string Iid { get; }
        Vector2 SpawnPosition { get; }
        int DirectionSign { get; }
        string LevelIId { get; }
    }
}