using UnityEngine;

namespace LDtkLevelManager
{
    public interface ICharacterLevelFlowSubject
    {
        void OnLevelExit();
        void OnLevelEnter();
        void PlaceInLevel(Vector2 position, int directionSign);
    }
}