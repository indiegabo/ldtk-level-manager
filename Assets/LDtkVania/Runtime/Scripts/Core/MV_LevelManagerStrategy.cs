using UnityEngine;

namespace LDtkVania
{
    public enum MV_LevelManagerStrategy
    {
        [InspectorName("Level and Neighbours")]
        Neighbours,
        [InspectorName("Entire Worlds")]
        Worlds,
        [InspectorName("Areas")]
        Areas,
    }
}