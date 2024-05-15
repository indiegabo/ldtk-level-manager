#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LDtkUnity;
using LDtkVania.Utils;
using System.Linq;

namespace LDtkVania
{
    public partial class MV_Project : ScriptableObject
    {
        #region Static

        public static string AddressablesProjectAddress = "LDtkVaniaProject";
        public static string AddressablesGroupName = "LDtkVania";
        public static string AddressablesLevelsLabel = "LDtkLevels";

        #endregion

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
                    string levelIid = ProcessLevelFile(assetPath, levelFile, world);

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

        public string ProcessLevelFile(string levelAssetPath, LDtkLevelFile levelFile, World world = null)
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
                project = this,
                iid = ldtkIid.Iid,
                assetPath = levelAssetPath,
                address = address,
                ldtkComponentLevel = componentLevel,
                asset = asset,
                ldtkFile = levelFile,
                world = world
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
                level.SetLeftBehind(false);
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

                AssetDatabase.RemoveObjectFromAsset(lostLevel);

                _lostLevels.Remove(lostLevel.Iid);
            }
        }

        public void SoftRemoveLevel(string iid)
        {
            if (!_levels.TryGetValue(iid, out MV_Level level)) return;

            _levels.Remove(iid);

            if (_lostLevels.ContainsKey(iid)) return;
            _lostLevels.Add(iid, level);
            level.SetLeftBehind(true);
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

        public MV_PaginatedResponse<MV_Level> GetPaginatedLevels(MV_LevelListFilters filters, MV_PaginationInfo pagination)
        {
            if (!string.IsNullOrEmpty(filters.world))
                filters.world = filters.world.ToLower();

            if (!string.IsNullOrEmpty(filters.area))
                filters.area = filters.area.ToLower();

            var query = _levels.Values;

            if (!string.IsNullOrEmpty(filters.world) && !string.IsNullOrEmpty(filters.area))
            {
                query.Where(level => level.AreaName.ToLower() == filters.area && level.WorldName.ToLower() == filters.world);
            }
            else if (!string.IsNullOrEmpty(filters.world))
            {
                query.Where(level => level.WorldName == filters.world);
            }

            var filteredLevels = query.ToList();
            int total = filteredLevels.Count;

            var result = filteredLevels
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .OrderBy(level => level.WorldName)
                .ThenBy(level => level.AreaName)
                .ThenBy(level => level.Name)
                .ToList();

            MV_PaginatedResponse<MV_Level> response = new()
            {
                TotalCount = total,
                Items = result,
            };

            return response;
        }
    }

    public struct MV_LevelListFilters
    {
        public string world;
        public string area;
    }
}
#endif