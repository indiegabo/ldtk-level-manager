using UnityEngine;

namespace LDtkVania
{
    public interface MV_IConnection
    {
        string Key { get; }
        string TargetLevelIid { get; }
        Vector2 SpawnPosition { get; }
        int DirectionSign { get; }
        void Activate();
        void Deactivate();
    }
}