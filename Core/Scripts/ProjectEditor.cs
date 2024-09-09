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

        public static string AddressablesProjectGroup = "LM_Projects";
        public static string AddressablesProjectLabel = "LM_Project";

        /// <summary>    
        /// [Editor only] <br/><br/>
        /// Finds all LDtkLevelManager projects in this Unity project.
        /// </summary>
        /// <returns>A list of all LDtkLevelManager projects in the project.</returns>
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


        /// <summary>
        /// [Editor only] <br/><br/>
        /// Used to ensure a clean project deletion.<br/>
        /// This method is called automatically when the user deletes the project asset.<br/>
        /// It iterates over all levels and removes any addressable labels or references to the deleted scene assets.<br/>
        /// </summary>
        public void ClearBeforeDeletion()
        {
            IEnumerable<LevelInfo> levels = _levels.Values.Concat(_lostLevels.Values);
            foreach (LevelInfo levelInfo in levels)
            {
                levelInfo.LevelFile.UnsetAdressable();

                if (!levelInfo.LeftBehind) continue;

                string path = AssetDatabase.GUIDToAssetPath(levelInfo.SceneInfo.AssetGuid);
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (sceneAsset != null)
                {
                    AssetDatabase.SetLabels(sceneAsset, new string[0]);
                    sceneAsset.UnsetAdressable();
                }
            }
        }

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Initializes the <see cref="LDtkLevelManager.Project"/> from an LDtk project file.<br/>
        /// This method is called automatically when creating a new LDtkLevelManager project.<br/>
        /// </summary>
        /// <param name="projectFile">The LDtk project file to initialize the project from.</param>
        public void Initialize(LDtkProjectFile projectFile)
        {
            _ldtkProjectFile = projectFile;
            ReSync();
            EvaluateWorldAreas();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Synchronizes the LDtk levels with the levels in the project and
        /// evaluates the worlds and areas of all levels in the project.<br/>
        /// This method is called automatically when the user edit the LDtk project from the LDtk app.<br/>
        /// </summary>
        public void ReSync()
        {
            if (_ldtkProjectFile == null) return;

            Dictionary<string, LDtkLevelFile> ldtkFiles = GenerateLdtkFilesDictionary();

            LdtkJson ldtkJson = _ldtkProjectFile.FromJson;

            if (!this.TrySetAsAddressable(
               $"{AddressablesProjectLabel}_{ldtkJson.Iid}",
               AddressablesProjectGroup,
               AddressablesProjectLabel
            ))
            {
                Logger.Error(
                    $"Failed to set project as addressable: {AddressablesProjectLabel}_{ldtkJson.Iid}",
                    this
                );
            }

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

            EvaluateWorldAreas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Generates a dictionary mapping LDtk level IIDs to their
        /// corresponding LDtkLevelFile assets.
        /// </summary>
        /// <returns>A dictionary mapping LDtk level IIDs to their
        /// corresponding LDtkLevelFile assets.</returns>
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

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Processes a level file and its associated Unity scene.
        /// 
        /// This method is used by the LDtkLevelManager to process levels and their associated
        /// scenes. It will set the level as addressable and create a LevelInfo object for it.
        /// If the level already exists, the LevelInfo object will be updated.
        /// 
        /// If the level has a scene, it will be regenerated and the addressable will be enforced.
        /// </summary>
        /// <param name="levelAssetPath">The path to the level asset.</param>
        /// <param name="levelFile">The LDtkLevelFile object.</param>
        /// <param name="world">(Optional) The world to which the level belongs to.</param>
        /// <returns>The IID of the level.</returns>
        public string ProcessLevelFile(string levelAssetPath, LDtkLevelFile levelFile, World world = null)
        {
            // Load the LDtkComponentLevel and LDtkIid components from the given path.
            (LDtkComponentLevel componentLevel, LDtkIid ldtkIid) = LoadLevelComponents(levelAssetPath);

            if (componentLevel == null || ldtkIid == null) return string.Empty;

            // Load the level asset.
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(levelAssetPath);
            if (asset == null) return string.Empty;

            // Generate the addressable address.
            string address = $"{LevelInfo.AdressableAddressPrexix}_{ldtkIid.Iid}";
            string groupName = LevelInfo.AddressableGroupName;
            string label = LevelInfo.AddressableLabel;

            // Try to set the level as addressable.
            if (!levelFile.TrySetAsAddressable(address, groupName, label))
            {
                Logger.Error(
                    $"Could not set level <color=#FFFFFF>{levelFile.name}</color> as addressable. "
                    + "Please check the console for errors.",
                    levelFile
                );
            }

            // Initialize the processing data.
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

            // Check if the level is already in the project.
            if (TryGetLevel(ldtkIid.Iid, out LevelInfo level))
            {
                UpdateLevelInfo(level, processingData);
                return ldtkIid.Iid;
            }

            // Check if the level is in the lost levels dictionary.
            if (_lostLevels.TryGetValue(ldtkIid.Iid, out LevelInfo lostLevel))
            {
                UpdateLevelInfo(lostLevel, processingData);
                AddLevel(lostLevel);
                return lostLevel.Iid;
            }

            // Create a new level.
            level = CreateInstance<LevelInfo>();
            level.name = asset.name;
            level.Initialize(processingData);
            AddLevel(level);

            // Method to update the level info.
            static void UpdateLevelInfo(LevelInfo levelInfo, LevelProcessingData processingData)
            {
                levelInfo.UpdateInfo(processingData);

                // If the level has a scene, regenerate it and enforce the addressable.
                if (levelInfo.WrappedInScene)
                {
                    LevelScene.RegenerateLevelObject(levelInfo);
                    LevelScene.EnforceSceneAddressable(levelInfo);
                }
            }

            // Return the IID of the level.
            return ldtkIid.Iid;
        }

        /// <summary>
        /// Loads the LDtkComponentLevel asset and LDtkIid component from the given path.
        /// </summary>
        /// <param name="levelAssetPath">The path to the level asset.</param>
        /// <returns>A tuple of the LDtkComponentLevel and LDtkIid components.</returns>
        (LDtkComponentLevel ComponentLevel, LDtkIid LdtkIid) LoadLevelComponents(string levelAssetPath)
        {
            LDtkComponentLevel componentLevel = AssetDatabase.LoadAssetAtPath<LDtkComponentLevel>(
                levelAssetPath
            );

            if (componentLevel == null) return (null, null);

            LDtkIid ldtkIid = componentLevel.GetComponent<LDtkIid>();

            return (componentLevel, ldtkIid);
        }

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Adds a level to the project.
        /// </summary>
        /// <remarks>
        /// If the level is already in the project, it will be updated.
        /// If the level is in the lost levels dictionary, it will be removed from there.
        /// </remarks>
        /// <param name="level">The level to add.</param>
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

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Removes a level from the project.
        /// </summary>
        /// <remarks>
        /// If the level has a scene, it will be destroyed.
        /// The level will be removed from the project and the lost levels dictionary.
        /// </remarks>
        /// <param name="iid">The IID of the level to remove.</param>
        public void RemoveLevel(string iid)
        {
            if (_levels.TryGetValue(iid, out LevelInfo level))
            {
                if (level.WrappedInScene)
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
                if (lostLevel.WrappedInScene)
                {
                    LevelScene.DestroySceneForLevel(lostLevel, false);
                }

                AssetDatabase.RemoveObjectFromAsset(lostLevel);

                _lostLevels.Remove(lostLevel.Iid);
            }
        }

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Soft removes a level from the project.
        /// </summary>
        /// <remarks>
        /// The level will be removed from the project, and added to the lost levels dictionary.
        /// The level will be marked as left behind.
        /// </remarks>
        /// <param name="iid">The IID of the level to soft remove.</param>
        public void SoftRemoveLevel(string iid)
        {
            if (!_levels.TryGetValue(iid, out LevelInfo level)) return;

            _levels.Remove(iid);

            if (_lostLevels.ContainsKey(iid)) return;
            _lostLevels.Add(iid, level);
            level.SetLeftBehind(true);
        }

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Clears the project by removing all levels and lost levels from the project.
        /// The levels will be removed from the project and the lost levels dictionary.
        /// The levels will be destroyed if they have a scene.
        /// The changes will be saved to disk.
        /// </summary>
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

        /// <summary>
        /// Returns a list of all levels that were left behind in the project.
        /// A level is considered "left behind" if its LDtk level file was not found in the project.
        /// </summary>
        /// <returns>A list of all levels that were left behind in the project.</returns>
        public List<LevelInfo> GetAllLeftBehind()
        {
            return _lostLevels.Values.ToList();
        }


        /// <summary>
        /// [Editor only] <br/><br/>
        /// Returns a paginated list of levels based on the given filters and pagination info.
        /// </summary>
        /// <param name="filters">The filters to apply to the levels.</param>
        /// <param name="pagination">The pagination info to use when getting the levels.</param>
        /// <returns>A paginated response containing the levels matching the filters and pagination info.</returns>
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

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Evaluates all the world areas in the project and returns a dictionary of <c>WorldInfo</c> instances.
        /// The dictionary is keyed by world name. Each <c>WorldInfo</c> instance contains the world identifier, world name, and a list of areas.
        /// The areas are sorted in alphabetical order.
        /// <remarks>
        /// This method is used by the editor to display the world areas in the project.
        /// </remarks>
        /// </summary>
        /// <returns>A dictionary of <c>WorldInfo</c> instances, keyed by world name.</returns>
        public WorldInfoDictionary EvaluateWorldAreas()
        {
            if (_ldtkProjectFile == null) return default;

            LdtkJson ldtkJson = _ldtkProjectFile.FromJson;
            _worldInfoRegistry.Clear();

            foreach (World world in ldtkJson.Worlds)
            {
                _worldInfoRegistry.Add(world.Identifier, new WorldInfo()
                {
                    iid = world.Iid,
                    name = world.Identifier,
                    areas = new List<string>(),
                });
            }

            foreach (LevelInfo level in _levels.Values)
            {
                if (string.IsNullOrEmpty(level.WorldName) || string.IsNullOrEmpty(level.AreaName)) continue;
                if (!_worldInfoRegistry.TryGetValue(level.WorldName, out WorldInfo info)) continue;
                if (info.areas.Contains(level.AreaName)) continue;
                info.areas.Add(level.AreaName);
            }

            EditorUtility.SetDirty(this);
            // AssetDatabase.SaveAssetIfDirty(this);

            return _worldInfoRegistry;
        }

        /// <summary>
        /// [Editor only] <br/><br/>
        /// Sets the navigation layer for the project.
        /// </summary>
        /// <param name="layerName">The name of the layer to set as the navigation layer.</param>
        public void SetNavigationLayer(string layerName)
        {
            _navigationLayer = layerName;
            EditorUtility.SetDirty(this);
            // AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
#endif