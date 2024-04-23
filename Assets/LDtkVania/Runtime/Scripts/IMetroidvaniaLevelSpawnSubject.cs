using UnityEngine;

namespace LDtkVania
{
    public interface IMetroidvaniaLevelSpawnSubject
    {
        void Spawn(Vector2 position, int directionSign);
    }
}