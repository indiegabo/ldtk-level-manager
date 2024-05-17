using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
using System.Linq;
using LDtkVania.Utils;

namespace LDtkVania
{
    [CreateAssetMenu(fileName = "LDtkVaniaProject", menuName = "LDtkVania/Project", order = 0)]
    public partial class MV_Project : ScriptableObject
    {

        #region Inspector

        [SerializeField]
        private LDtkProjectFile _ldtkProjectFile;

        [SerializeField]
        private bool _syncLevelsAtCompile = true;

        [SerializeField]
        private string _connectionsContainerName;

        [SerializeField]
        private string _checkpointsContainerName;

        [SerializeField]
        private MV_LevelsDictionary _levels = new();

        [SerializeField]
        private MV_LevelsDictionary _lostLevels = new();

        [SerializeField]
        private MV_WorldAreasDictionary _worldAreas = new();

        #endregion

        #region Fields

        private LdtkJson _ldtkProject;

        #endregion

        #region Getters

        public bool SyncLevelsAtCompile => _syncLevelsAtCompile;
        public string ConnectionsContainerName => "Connections";
        public string CheckpointsContainerName => "Checkpoints";

        public bool IsInitialized => _ldtkProjectFile != null;
        public LDtkProjectFile LDtkProjectFile => _ldtkProjectFile;
        public LdtkJson LDtkProject => _ldtkProject ??= _ldtkProjectFile.FromJson;
        public int PixelsPerUnit => LDtkProject.DefaultGridSize;
        public MV_WorldAreasDictionary WorldAreas => _worldAreas;

        #endregion

        #region Providing Levels

        public MV_Level GetLevel(string iid)
        {
            if (!_levels.ContainsKey(iid)) return null;
            return _levels[iid];
        }

        public bool TryGetLevel(string iid, out MV_Level mvLevel)
        {
            return _levels.TryGetValue(iid, out mvLevel);
        }

        public bool HasLevel(string iid) => _levels.ContainsKey(iid);

        public List<MV_Level> GetAllLevels()
        {
            return _levels.Values.ToList();
        }

        #endregion

        #region World and areas

        public List<MV_WorldAreas> GetAllWorldAreas()
        {
            return _worldAreas.Values.ToList();
        }

        public MV_WorldAreas GetWorldAreas(string worldName)
        {
            if (!_worldAreas.ContainsKey(worldName)) return null;
            return _worldAreas[worldName];
        }

        public bool TryGetWorldAreas(string worldName, out MV_WorldAreas mvWorldAreas)
        {
            return _worldAreas.TryGetValue(worldName, out mvWorldAreas);
        }

        public bool TryGetWorldAreas(MV_Level level, out MV_WorldAreas mvWorldAreas)
        {
            if (string.IsNullOrEmpty(level.WorldName))
            {
                MV_Logger.Warning($"Trying to get world areas for level {level.Name}({level.Iid}) but it has no world name defined", this);
                mvWorldAreas = null;
                return false;
            }
            return _worldAreas.TryGetValue(level.WorldName, out mvWorldAreas);
        }

        #endregion

    }
}