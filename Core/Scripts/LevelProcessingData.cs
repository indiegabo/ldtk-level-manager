using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
    /// <summary>
    /// [Editor Only] <br /><br />
    /// Holds all the information needed to process a level.
    /// </summary>
    public class LevelProcessingData
    {
        public Project project;
        public string iid;
        public string assetPath;
        public string address;
        public LDtkComponentLevel ldtkComponentLevel;
        public Object asset;
        public LDtkLevelFile ldtkFile;
        public World world;
    }
}