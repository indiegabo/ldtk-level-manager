using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Linq;
using LDtkVania.Utils;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LDtkVania
{
    [CreateAssetMenu(fileName = "LDtkVaniaProject", menuName = "LDtkVania/Project", order = 0)]
    public partial class MV_Project : ScriptableObject
    {
        #region Static

        public static string AddressablesProjectAddress = "LDtkVaniaProject";
        public static string AddressablesGroupName = "LDtkVania";
        public static string AddressablesLevelsLabel = "LDtkLevels";

        #endregion

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
        private MV_AreasDictionary _areasLevels;

        #endregion

        #region Fields

        private LdtkJson _ldtkProject;

        #endregion

        #region Getters

        public bool SyncLevelsAtCompile => _syncLevelsAtCompile;
        public string ConnectionsContainerName => "Connections";
        public string CheckpointsContainerName => "Checkpoints";

        public LdtkJson LDtkProject => _ldtkProject ??= _ldtkProjectFile.FromJson;
        public int PixelsPerUnit => LDtkProject.DefaultGridSize;

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

        public List<MV_Level> GetLevels()
        {
            return _levels.Values.ToList();
        }

        #endregion

        public void CreateHashSet()
        {
            _areasLevels = new MV_AreasDictionary();
            List<string> areasNames = new()
            {
                "Area 1",
                "Area 2",
                "Area 3"
            };

            foreach (string area in areasNames)
            {
                _areasLevels[area] = new AreaLevelList();
                for (int i = 0; i < areasNames.Count; i++)
                {
                    string levelName = areasNames[i] + "_" + i;
                    _areasLevels[area].Add(levelName);
                }
            }
        }

        public void LogHashSet()
        {
            foreach (var pair in _areasLevels)
            {
                Debug.Log($"Area: {pair.Key}:");
                foreach (var levelName in pair.Value.Levels)
                {
                    Debug.Log($"Level: {levelName}");
                }
            }
        }

        public void DestroyHashSet()
        {
            _areasLevels.Clear();
        }

        #region Classes

        [System.Serializable]
        public class MV_LevelsDictionary : SerializedDictionary<string, MV_Level> { }

        [System.Serializable]
        public class MV_AreasDictionary : SerializedDictionary<string, AreaLevelList> { }

        [System.Serializable]
        public class AreaLevelList
        {
            [SerializeField]
            private List<string> _levels = new();

            public List<string> Levels => _levels;

            public void Add(string level)
            {
                if (_levels.Contains(level)) return;
                _levels.Add(level);
            }

            public void Clear()
            {
                _levels.Clear();
            }

            public void Remove(string level)
            {
                _levels.Remove(level);
            }
        }

        #endregion

#if UNITY_EDITOR
        #region Editor Only

        public void SyncLevels()
        {
            LdtkJson ldtkProjectJson = _ldtkProjectFile.FromJson;

            HashSet<string> presentLevels = new();

            AsyncOperationHandle handle = Addressables.LoadResourceLocationsAsync(AddressablesLevelsLabel);
            handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded) return;

            IList<IResourceLocation> locations = (IList<IResourceLocation>)handle.Result;

            foreach (IResourceLocation location in locations)
            {
                string levelIid = CreateOrUpdateBasedOnLocation(ldtkProjectJson, location);
                if (string.IsNullOrEmpty(levelIid)) continue;

                presentLevels.Add(levelIid);
            }

            var levelsToRemove = new List<string>();

            foreach (var pair in _levels)
            {
                if (!presentLevels.Contains(pair.Value.Iid))
                {
                    levelsToRemove.Add(pair.Value.Iid);
                }
            }

            // Removing in a separate step to avoid modifying the collection while iterating
            foreach (string iid in levelsToRemove)
            {
                SoftRemoveLevel(iid);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private string CreateOrUpdateBasedOnLocation(LdtkJson projectJSON, IResourceLocation location)
        {
            LDtkComponentLevel componentLevel = AssetDatabase.LoadAssetAtPath<LDtkComponentLevel>(location.InternalId);
            if (componentLevel == null) return null;

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(location.InternalId);
            if (asset == null) return null;

            LDtkLevelFile file = AssetDatabase.LoadAssetAtPath<LDtkLevelFile>(location.InternalId);
            if (file == null) return null;

            Level ldtkLevel = file.FromJson;
            if (TryGetLevel(ldtkLevel.Iid, out MV_Level level))
            {
                level.UpdateInfo(projectJSON, componentLevel, location, asset, file);
                return level.Iid;
            }

            level = CreateInstance<MV_Level>();
            level.name = asset.name;
            level.Initialize(projectJSON, componentLevel, location, asset, file);
            AddLevel(level);

            return level.Iid;
        }

        public void AddLevel(MV_Level level)
        {
            if (!_levels.ContainsKey(level.Iid))
            {
                _levels.Add(level.Iid, level);
                AssetDatabase.AddObjectToAsset(level, this);
            }
            else
            {
                _levels[level.Iid] = level;
            }

        }

        public void RemoveLevel(MV_Level level)
        {
            if (level == null || !_levels.ContainsKey(level.Iid)) return;

            if (level.HasScene)
            {
                MV_LevelScene.DestroySceneForLevel(level, false);
            }

            AssetDatabase.RemoveObjectFromAsset(level);

            if (_levels.ContainsKey(level.Iid))
            {
                _levels.Remove(level.Iid);
            }
        }

        public void RemoveLevel(string iid)
        {
            if (!_levels.ContainsKey(iid)) return;
            MV_Level level = _levels[iid];
            RemoveLevel(level);
        }

        public void SoftRemoveLevel(string iid)
        {
            if (!_levels.TryGetValue(iid, out MV_Level level)) return;

            _levels.Remove(iid);

            if (_lostLevels.ContainsKey(iid)) return;
            _lostLevels.Add(iid, level);
        }

        public void HardClear()
        {
            GetLevels().ForEach(level =>
            {
                AssetDatabase.RemoveObjectFromAsset(level);
            });

            foreach (MV_Level level in _lostLevels.Values)
            {
                AssetDatabase.RemoveObjectFromAsset(level);
            }

            _levels.Clear();
            _lostLevels.Clear();
        }

        #endregion
#endif
    }
}