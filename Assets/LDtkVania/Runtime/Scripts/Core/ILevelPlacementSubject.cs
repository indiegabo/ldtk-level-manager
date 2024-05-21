using UnityEngine;

namespace LDtkVania
{
    public interface ILevelPlacementSubject
    {
        void PlaceInLevel(Vector2 position, int directionSign);
    }
}