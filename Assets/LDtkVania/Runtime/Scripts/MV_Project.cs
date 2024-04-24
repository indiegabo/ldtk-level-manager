using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

namespace LDtkVania
{
    public partial class MV_Project : ScriptableObject
    {
        #region Static

        public static string AddressablesProjectAddress = "LDtkVaniaProject";
        public static string AddressablesGroupName = "LDtkVania";
        public static string AddressablesLevelsLabel = "LDtkLevels";

        private static MV_Project _instance;
        public static MV_Project Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    if (!FindExistingProjectAsset(out _instance))
                    {
                        _instance = CreateProject();
                    }

                    if (_instance == null)
                    {
                        MV_Logger.Error($"No instance of the LDtkVaniaProject was found.");
                    }
#else
                    if (!FindExistingProjectAsset(out _instance))
                    {
                        MV_Logger.Error($"No instance of the LDtkVaniaProject was found.");
                    }
#endif

                }

                return _instance;
            }
        }

        public static bool SilentInstanceCheck
        {
            get
            {
                if (_instance == null)
                {
                    return FindExistingProjectAsset(out _instance);
                }

                return true;
            }
        }

        private static bool FindExistingProjectAsset(out MV_Project project)
        {
            var addressOp = Addressables.LoadResourceLocationsAsync(AddressablesProjectAddress);
            var locations = addressOp.WaitForCompletion();

            if (locations != null && locations.Count > 0)
            {
                IResourceLocation loc = locations[0];
                var op = Addressables.LoadAssetAsync<MV_Project>(loc);
                project = op.WaitForCompletion(); //Forces synchronous load so that we can return immediately
                return project != null;
            }

            project = null;
            return false;
        }

#if UNITY_EDITOR

        private static MV_Project CreateProject()
        {
            if (Application.isPlaying)
            {
                MV_Logger.Error("Trying to create a LDtkVaniaProject in play mode.");
                return null;
            }

            MV_Project project = CreateInstance<MV_Project>();

            string assetName = AddressablesProjectAddress;
            AssetDatabase.CreateAsset(project, $"Assets/{assetName}.asset");
            EditorUtility.SetDirty(project);
            AssetDatabase.SaveAssetIfDirty(project);

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(project));
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            string groupName = AddressablesGroupName;
            var group = settings.FindGroup(groupName);
            if (!group)
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            settings.AddLabel(AddressablesLevelsLabel);

            AssetReference assetReference = settings.CreateAssetReference(guid);
            assetReference.SetEditorAsset(project);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);

            // This will fail if asset is being created on AssetPostprocessor
            if (entry != null)
            {
                entry.SetAddress(AddressablesProjectAddress);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            }
            else
            {
                MV_Logger.Error($"Could not create AddressableAssetEntry for {assetName}");
                return null;
            }

            EditorUtility.SetDirty(project);
            AssetDatabase.SaveAssetIfDirty(project);
            AssetDatabase.Refresh();

            MV_Logger.Message($"Created new LDtkVaniaProject at {AssetDatabase.GetAssetPath(project)}.");

            return project;
        }
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
        private List<AssetLabelReference> _levelsLabels = new();

        [SerializeField]
        private MV_LevelsDictionary _levels = new();

        #endregion

        #region Getters

        public bool SyncLevelsAtCompile => _syncLevelsAtCompile;
        public string ConnectionsContainerName => _connectionsContainerName;
        public string CheckpointsContainerName => _checkpointsContainerName;
        public List<AssetLabelReference> LevelsLabels => _levelsLabels;

        public LdtkJson LDtkProject => _ldtkProjectFile.FromJson;
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


        public bool HasLevel(string iid)
        {
            return _levels.ContainsKey(iid);
        }

        public List<MV_Level> GetLevels()
        {
            return _levels.Values.ToList();
        }

        #endregion

        #region Classes

        [System.Serializable]
        public class MV_LevelsDictionary : SerializedDictionary<string, MV_Level> { }

        #endregion

#if UNITY_EDITOR
        #region Editor Only

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
                Remove(iid);
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
            if (TryGetLevel(ldtkLevel.Iid, out MV_Level mvLevel))
            {
                mvLevel.UpdateInfo(componentLevel, location, asset, file);
                return mvLevel.Iid;
            }

            mvLevel = CreateInstance<MV_Level>();
            mvLevel.name = asset.name;
            mvLevel.Initialize(componentLevel, location, asset, file);
            Add(mvLevel);

            return mvLevel.Iid;
        }
        public void Add(MV_Level level)
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

        public void Remove(MV_Level level)
        {
            if (level == null) return;

            if (_levels.ContainsKey(level.Iid))
            {
                _levels.Remove(level.Iid);
            }

            AssetDatabase.RemoveObjectFromAsset(level);
            // AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(level));
        }

        public void Remove(string iid)
        {
            if (!_levels.ContainsKey(iid)) return;
            MV_Level level = _levels[iid];
            Remove(level);
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