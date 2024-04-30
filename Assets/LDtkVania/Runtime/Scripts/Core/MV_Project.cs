using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Linq;

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
        private List<AssetLabelReference> _levelsLabels = new();

        [SerializeField]
        private MV_LevelsDictionary _levels = new();

        [SerializeField]
        private MV_LevelsDictionary _lostLevels = new();

        [SerializeField]
        private MV_WorldsDictionary _worlds = new();

        #endregion

        #region Fields

        private LdtkJson _ldtkProject;

        #endregion

        #region Getters

        public bool SyncLevelsAtCompile => _syncLevelsAtCompile;
        public string ConnectionsContainerName => "Connections";
        public string CheckpointsContainerName => "Checkpoints";
        public List<AssetLabelReference> LevelsLabels => _levelsLabels;

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

        public bool HasWorld(string iid) => _worlds.ContainsKey(iid);

        public List<MV_World> GetWorlds()
        {
            return _worlds.Values.ToList();
        }

        #endregion

        #region Classes

        [System.Serializable]
        public class MV_LevelsDictionary : SerializedDictionary<string, MV_Level> { }

        [System.Serializable]
        public class MV_WorldsDictionary : SerializedDictionary<string, MV_World> { }

        #endregion

#if UNITY_EDITOR
        #region Editor Only

        public void SyncWorlds(IEnumerable<World> worlds)
        {
            HashSet<string> existingIids = new();

            foreach (World world in worlds)
            {
                if (_worlds.ContainsKey(world.Iid))
                {
                    _worlds[world.Iid].UpdateInfo(world);
                }
                else
                {
                    _worlds.Add(world.Iid, new MV_World(world));
                }

                existingIids.Add(world.Iid);
            }

            HashSet<string> toRemoveIids = new();

            foreach (string iid in _worlds.Keys)
            {
                if (existingIids.Contains(iid)) continue;
                toRemoveIids.Add(iid);
            }

            foreach (string iid in toRemoveIids)
            {
                _worlds.Remove(iid);
            }
        }

        public void SyncLevels()
        {
            HashSet<string> presentLevels = new();

            AsyncOperationHandle handle = Addressables.LoadResourceLocationsAsync(AddressablesLevelsLabel);
            handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded) return;

            IList<IResourceLocation> locations = (IList<IResourceLocation>)handle.Result;

            foreach (IResourceLocation location in locations)
            {
                string levelIid = HandleLevelLocation(location);
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

        private string HandleLevelLocation(IResourceLocation location)
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
                level.UpdateInfo(componentLevel, location, asset, file);
                return level.Iid;
            }

            level = CreateInstance<MV_Level>();
            level.name = asset.name;
            level.Initialize(componentLevel, location, asset, file);
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

        public void Clear()
        {
            GetLevels().ForEach(level =>
            {
                AssetDatabase.RemoveObjectFromAsset(level);
            });
            _levels.Clear();
        }

        #endregion
#endif
    }
}