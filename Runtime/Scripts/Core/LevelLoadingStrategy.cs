using UnityEngine;

namespace LDtkLevelManager
{
    public enum LevelLoadingStrategy
    {
        [InspectorName("Level and Neighbours")]
        Neighbours,
        [InspectorName("Entire Worlds")]
        Worlds,
        [InspectorName("Areas")]
        Areas,
    }
}