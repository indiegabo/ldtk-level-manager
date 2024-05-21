using UnityEngine;

namespace LDtkVania
{
    public interface ILevelAnchor
    {
        string Iid { get; }
        string LevelIId { get; }
        Vector2 SpawnPoint { get; }
        int FacingSign { get; }
    }
}