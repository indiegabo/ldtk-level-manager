using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
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

#if UNITY_EDITOR
#endif
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

        public bool HasProjectFile => _ldtkProjectFile != null;
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

        public List<MV_Level> GetAllLevels()
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
            if (_ldtkProjectFile == null) return;

            Dictionary<string, LDtkLevelFile> ldtkFiles = GenerateLdtkFilesDictionary();

            LdtkJson ldtkJson = _ldtkProjectFile.FromJson;
            HashSet<string> presentLevels = new();

            foreach (World world in ldtkJson.Worlds)
            {
                foreach (Level level in world.Levels)
                {
                    if (!ldtkFiles.TryGetValue(level.Iid, out LDtkLevelFile levelFile)) continue;

                    string assetPath = AssetDatabase.GetAssetPath(levelFile);
                    string levelIid = ProcessLevelFile(assetPath, levelFile);

                    if (string.IsNullOrEmpty(levelIid)) continue;

                    presentLevels.Add(levelIid);
                }
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

        private Dictionary<string, LDtkLevelFile> GenerateLdtkFilesDictionary()
        {
            Dictionary<string, LDtkLevelFile> levels = new();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(LDtkLevelFile)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LDtkLevelFile file = AssetDatabase.LoadAssetAtPath<LDtkLevelFile>(path);
                if (file == null)
                {
                    continue;
                }

                levels.Add(file.FromJson.Iid, file);
            }

            return levels;
        }

        public string ProcessLevelFile(string levelAssetPath, LDtkLevelFile levelFile)
        {
            LDtkComponentLevel componentLevel = AssetDatabase.LoadAssetAtPath<LDtkComponentLevel>(levelAssetPath);
            if (componentLevel == null) return string.Empty;

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(levelAssetPath);
            if (asset == null) return string.Empty;

            LDtkIid ldtkIid = componentLevel.GetComponent<LDtkIid>();

            string address = $"{MV_Level.AdressableAddressPrexix}_{ldtkIid.Iid}";
            string groupName = MV_Level.AddressableGroupName;
            string label = MV_Level.AddressableLabel;

            if (!levelFile.TrySetAsAddressable(address, groupName, label))
            {
                MV_Logger.Error($"Could not set level <color=#FFFFFF>{levelFile.name}</color> as addressable. Please check the console for errors.", levelFile);
            }

            MV_LevelProcessingData processingData = new()
            {
                iid = ldtkIid.Iid,
                assetPath = levelAssetPath,
                address = address,
                ldtkComponentLevel = componentLevel,
                asset = asset,
                ldtkFile = levelFile
            };

            if (TryGetLevel(ldtkIid.Iid, out MV_Level level))
            {
                level.UpdateInfo(processingData);
                return ldtkIid.Iid;
            }

            if (_lostLevels.TryGetValue(ldtkIid.Iid, out MV_Level lostLevel))
            {
                lostLevel.UpdateInfo(processingData);
                AddLevel(lostLevel);
                return lostLevel.Iid;
            }

            level = CreateInstance<MV_Level>();
            level.name = asset.name;
            level.Initialize(processingData);
            AddLevel(level);

            return ldtkIid.Iid;
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

            if (_lostLevels.ContainsKey(level.Iid))
            {
                _lostLevels.Remove(level.Iid);
            }

        }

        public void RemoveLevel(string iid)
        {
            if (_levels.TryGetValue(iid, out MV_Level level))
            {
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

            if (_lostLevels.TryGetValue(iid, out MV_Level lostLevel))
            {
                if (lostLevel.HasScene)
                {
                    MV_LevelScene.DestroySceneForLevel(lostLevel, false);
                }

                _lostLevels.Remove(lostLevel.Iid);
            }
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
            GetAllLevels().ForEach(level =>
            {
                if (level == null) return;
                AssetDatabase.RemoveObjectFromAsset(level);
            });

            foreach (MV_Level level in _lostLevels.Values)
            {
                if (level == null) return;
                AssetDatabase.RemoveObjectFromAsset(level);
            }

            _levels.Clear();
            _lostLevels.Clear();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Project has been cleared!");
            Debug.Log($"Levels: {_levels.Count}");
            Debug.Log($"Lost levels: {_lostLevels.Count}");
        }

        public List<MV_Level> GetAllLeftBehind()
        {
            return _lostLevels.Values.ToList();
        }

        #endregion
#endif
    }
}