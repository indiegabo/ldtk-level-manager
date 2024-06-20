using UnityEngine;

namespace LDtkVania
{
    public interface ICharacterLevelFlowSubject
    {
        void OnLevelExit();
        void OnLevelEnter();
        void PlaceInLevel(Vector2 position, int directionSign);
    }
}