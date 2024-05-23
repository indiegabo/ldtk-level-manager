using UnityEngine;

namespace LDtkVania
{
    public enum MV_LevelLoadingStrategy
    {
        [InspectorName("Level and Neighbours")]
        Neighbours,
        [InspectorName("Entire Worlds")]
        Worlds,
        [InspectorName("Areas")]
        Areas,
    }
}