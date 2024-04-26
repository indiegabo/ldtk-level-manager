using UnityEngine;

namespace LDtkVania
{
    public interface ILevelSpawnSubject
    {
        void Spawn(Vector2 position, int directionSign);
    }
}