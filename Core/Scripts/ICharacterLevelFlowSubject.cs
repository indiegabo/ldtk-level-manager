using UnityEngine;

namespace LDtkLevelManager
{
    /// <summary>
    /// Implement this interface on your character to receive notifications from the
    /// <see cref="LevelBehaviour"/> about the level's state. This is needed to know when to
    /// enable or disable the character's controls, for example.
    /// </summary>
    public interface ICharacterLevelFlowSubject
    {
        /// <summary>
        /// Called when the level is exited, meaning that the player should lose 
        /// control of the character and a new level will be loaded.
        /// </summary>
        void OnLevelExit();

        /// <summary>
        /// Called when the level is entered, meaning that the player should
        /// regain control of the character.
        /// </summary>
        void OnLevelEnter();

        /// <summary>
        /// Called uppon level preparation in order place the player character in the level at the specified position 
        /// and with the specified facing direction sign (-1 for left, 1 for right).
        /// </summary>
        /// <param name="position">The position to place the player in.</param>
        /// <param name="facingDirectionSign">The direction wich the player should be facing</param>
        void PlaceInLevel(Vector2 position, int facingDirectionSign);
    }
}