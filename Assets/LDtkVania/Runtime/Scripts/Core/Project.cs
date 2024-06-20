using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
using System.Linq;

namespace LDtkVania
{
    public partial class Project : ScriptableObject
    {

        #region Serializing

        [SerializeField]
        private LDtkProjectFile _ldtkProjectFile;

        [SerializeField]
        private bool _syncLevelsAtCompile = true;

        [SerializeField]
        private string _navigationLayer;

        [SerializeField]
        private InfoDictionary _levels = new();

        [SerializeField]
        private InfoDictionary _lostLevels = new();

        [SerializeField]
        private WorldInfoDictionary _worldAreas = new();

        #endregion

        #region Fields

        private LdtkJson _ldtkProject;

        #endregion

        #region Getters

        public bool SyncLevelsAtCompile => _syncLevelsAtCompile;
        public string NavigationLayer => _navigationLayer;

        public bool IsInitialized => _ldtkProjectFile != null;
        public int LevelsCount => _levels.Count;
        public LDtkProjectFile LDtkProjectFile => _ldtkProjectFile;
        public LdtkJson LDtkProject => _ldtkProject ??= _ldtkProjectFile.FromJson;
        public int PixelsPerUnit => LDtkProject.DefaultGridSize;
        public WorldInfoDictionary WorldAreas => _worldAreas;

        #endregion

        #region Providing Levels

        public LevelInfo GetLevel(string iid)
        {
            if (!_levels.ContainsKey(iid)) return null;
            return _levels[iid];
        }

        public bool TryGetLevel(string iid, out LevelInfo levelInfo)
        {
            return _levels.TryGetValue(iid, out levelInfo);
        }

        public bool HasLevel(string iid) => _levels.ContainsKey(iid);

        public List<LevelInfo> GetAllLevels()
        {
            return _levels.Values.ToList();
        }

        #endregion

        #region World and areas

        public List<LevelInfo> GetAllLevelsInWorld(string worldName)
        {
            if (!_worldAreas.ContainsKey(worldName)) return null;

            List<LevelInfo> levels = new();
            foreach (LevelInfo level in _levels.Values)
            {
                if (level.WorldName == worldName) levels.Add(level);
            }
            return levels;
        }

        public HashSet<string> GetAllLevelsIidsInWorld(string worldName)
        {
            if (!_worldAreas.ContainsKey(worldName)) return null;

            HashSet<string> iids = new();
            foreach (LevelInfo level in _levels.Values)
            {
                if (level.WorldName == worldName) iids.Add(level.Iid);
            }

            return iids;
        }

        public HashSet<string> GetAllLevelsIidsInArea(string areaName)
        {
            HashSet<string> iids = new();
            foreach (LevelInfo level in _levels.Values)
            {
                if (level.AreaName == areaName) iids.Add(level.Iid);
            }

            return iids;
        }

        public List<WorldInfo> GetAllWorldAreas()
        {
            return _worldAreas.Values.ToList();
        }

        public WorldInfo GetWorldAreas(string worldName)
        {
            if (!_worldAreas.ContainsKey(worldName)) return null;
            return _worldAreas[worldName];
        }

        public bool TryGetWorldAreas(string worldName, out WorldInfo mvWorldAreas)
        {
            return _worldAreas.TryGetValue(worldName, out mvWorldAreas);
        }

        public bool TryGetWorldAreas(LevelInfo level, out WorldInfo mvWorldAreas)
        {
            if (string.IsNullOrEmpty(level.WorldName))
            {
                Logger.Warning($"Trying to get world areas for level {level.Name}({level.Iid}) but it has no world name defined.", this);
                mvWorldAreas = null;
                return false;
            }
            return _worldAreas.TryGetValue(level.WorldName, out mvWorldAreas);
        }

        #endregion

    }
}