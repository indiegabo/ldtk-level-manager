using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
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