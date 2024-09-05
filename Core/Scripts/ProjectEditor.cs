#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LDtkUnity;
using LDtkLevelManager.Utils;
using System.Linq;

namespace LDtkLevelManager
{
    public partial class Project : ScriptableObject
    {
        #region Static

        public static string AddressablesProjectAddress = "LDtkLevelManagerProject";
        public static string AddressablesGroupName = "LDtkLevelManager";
        public static string AddressablesLevelsLabel = "LDtkLevels";

        public static List<Project> FindAllProjects()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(Project)}");

            List<Project> projects = new();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Project project = AssetDatabase.LoadAssetAtPath<Project>(path);
                if (project != null)
                {
                    projects.Add(project);
                }
            }

            return projects;
        }

        #endregion


        // Settings
        public void ClearBeforeDeletion()
        {
            IEnumerable<LevelInfo> levels = _levels.Values.Concat(_lostLevels.Values);
            foreach (LevelInfo levelInfo in levels)
            {
                levelInfo.LevelFile.UnsetAdressable();

                if (!levelInfo.LeftBehind) continue;

                string path = AssetDatabase.GUIDToAssetPath(levelInfo.Scene.AssetGuid);
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (sceneAsset != null)
                {
                    AssetDatabase.SetLabels(sceneAsset, new string[0]);
                    sceneAsset.UnsetAdressable();
                }
            }
        }

        public void Initialize(LDtkProjectFile projectFile)
        {
            _ldtkProjectFile = projectFile;
            SyncLevels();
            EvaluateWorldAreas();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

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

            string address = $"{LevelInfo.AdressableAddressPrexix}_{ldtkIid.Iid}";
            string groupName = LevelInfo.AddressableGroupName;
            string label = LevelInfo.AddressableLabel;

            if (!levelFile.TrySetAsAddressable(address, groupName, label))
            {
                Logger.Error($"Could not set level <color=#FFFFFF>{levelFile.name}</color> as addressable. Please check the console for errors.", levelFile);
            }

            LevelProcessingData processingData = new()
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

            if (TryGetLevel(ldtkIid.Iid, out LevelInfo level))
            {
                level.UpdateInfo(processingData);

                if (level.HasScene)
                {
                    LevelScene.EnforceSceneAddressable(level);
                }

                return ldtkIid.Iid;
            }

            if (_lostLevels.TryGetValue(ldtkIid.Iid, out LevelInfo lostLevel))
            {
                lostLevel.UpdateInfo(processingData);
                if (lostLevel.HasScene)
                {
                    LevelScene.EnforceSceneAddressable(lostLevel);
                }
                AddLevel(lostLevel);
                return lostLevel.Iid;
            }

            level = CreateInstance<LevelInfo>();
            level.name = asset.name;
            level.Initialize(processingData);
            AddLevel(level);

            return ldtkIid.Iid;
        }

        public void AddLevel(LevelInfo level)
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
            if (_levels.TryGetValue(iid, out LevelInfo level))
            {
                if (level.HasScene)
                {
                    LevelScene.DestroySceneForLevel(level, false);
                }

                AssetDatabase.RemoveObjectFromAsset(level);

                if (_levels.ContainsKey(level.Iid))
                {
                    _levels.Remove(level.Iid);
                }
            }

            if (_lostLevels.TryGetValue(iid, out LevelInfo lostLevel))
            {
                if (lostLevel.HasScene)
                {
                    LevelScene.DestroySceneForLevel(lostLevel, false);
                }

                AssetDatabase.RemoveObjectFromAsset(lostLevel);

                _lostLevels.Remove(lostLevel.Iid);
            }
        }

        public void SoftRemoveLevel(string iid)
        {
            if (!_levels.TryGetValue(iid, out LevelInfo level)) return;

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

            foreach (LevelInfo level in _lostLevels.Values)
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

        public List<LevelInfo> GetAllLeftBehind()
        {
            return _lostLevels.Values.ToList();
        }

        /// <summary>
        /// Returns a paginated list of levels filtered by world, area and level name.
        /// </summary>
        /// <param name="filters">The filters to apply</param>
        /// <param name="pagination">The pagination info</param>
        /// <returns>A paginated list of levels</returns>
        public PaginatedResponse<LevelInfo> GetPaginatedLevels(LevelListFilters filters, PaginationInfo pagination)
        {
            // Normalize the input filter values to lower case
            if (!string.IsNullOrEmpty(filters.world))
                filters.world = filters.world.ToLower();

            if (!string.IsNullOrEmpty(filters.area))
                filters.area = filters.area.ToLower();

            if (!string.IsNullOrEmpty(filters.levelName))
                filters.levelName = filters.levelName.ToLower();

            // Get all levels
            List<LevelInfo> filteredLevels = _levels.Values.ToList();

            // Apply world and area filters
            if (!string.IsNullOrEmpty(filters.world) && !string.IsNullOrEmpty(filters.area))
            {
                filteredLevels = _levels.Values.Where(level =>
                {
                    if (string.IsNullOrEmpty(level.AreaName) || string.IsNullOrEmpty(level.WorldName)) return false;
                    return level.AreaName.ToLower() == filters.area
                    && level.WorldName.ToLower() == filters.world;
                }).ToList();
            }
            else if (!string.IsNullOrEmpty(filters.world))
            {
                filteredLevels = _levels.Values.Where(level =>
                {
                    if (string.IsNullOrEmpty(level.WorldName)) return false;
                    return level.WorldName.ToLower() == filters.world;
                }).ToList();
            }

            // Apply level name filter
            if (!string.IsNullOrEmpty(filters.levelName))
            {
                filteredLevels = filteredLevels.Where(level => level.Name.ToLower().Contains(filters.levelName.ToLower())).ToList();
            }

            int total = filteredLevels.Count;

            // Get the levels for the current page
            var result = filteredLevels
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .OrderBy(level => level.WorldName)
                .ThenBy(level => level.AreaName)
                .ThenBy(level => level.Name)
                .ToList();

            // Create the paginated response
            PaginatedResponse<LevelInfo> response = new()
            {
                TotalCount = total,
                Items = result,
            };

            return response;
        }

        public WorldInfoDictionary EvaluateWorldAreas()
        {
            if (_ldtkProjectFile == null) return default;

            LdtkJson ldtkJson = _ldtkProjectFile.FromJson;
            _worldAreas.Clear();

            foreach (World world in ldtkJson.Worlds)
            {
                _worldAreas.Add(world.Identifier, new WorldInfo()
                {
                    worldIid = world.Iid,
                    worldName = world.Identifier,
                    areas = new List<string>(),
                });
            }

            foreach (LevelInfo level in _levels.Values)
            {
                if (string.IsNullOrEmpty(level.WorldName) || string.IsNullOrEmpty(level.AreaName)) continue;
                if (!_worldAreas.TryGetValue(level.WorldName, out WorldInfo worldAreas)) continue;
                if (worldAreas.areas.Contains(level.AreaName)) continue;
                worldAreas.areas.Add(level.AreaName);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);

            return _worldAreas;
        }

        public void SetNavigationLayer(string layerName)
        {
            _navigationLayer = layerName;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
#endif