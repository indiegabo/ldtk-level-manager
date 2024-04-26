using UnityEngine;

namespace LDtkVania
{
    public interface MV_ILevelSpawnSubject
    {
        void Spawn(Vector2 position, int directionSign);
    }
}