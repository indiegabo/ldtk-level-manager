using UnityEngine;

namespace LDtkVania
{
    public interface IConnection
    {
        string Key { get; }
        string TargetLevelIid { get; }
        Vector2 SpawnPoint { get; }
        int FacingSign { get; }

        void Initialize();
        void Activate();
        void Deactivate();
    }
}