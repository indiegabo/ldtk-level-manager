using UnityEngine;
using System.Collections.Generic;
using LDtkUnity;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

namespace LDtkVania
{
    public class MV_Project : ScriptableObject
    {
        #region Static

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

        private static bool FindExistingProjectAsset(out MV_Project project)
        {
            var addressOp = Addressables.LoadResourceLocationsAsync("LDtkVaniaProject");
            var locations = addressOp.WaitForCompletion();

            if (locations != null && locations.Count > 0)
            {
                IResourceLocation loc = locations[0];
                var op = Addressables.LoadAssetAsync<MV_Project>(loc);
                project = op.WaitForCompletion(); //Forces synchronous load so that we can return immediately
                return true;
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

            string assetName = "LDtkVaniaProject";
            AssetDatabase.CreateAsset(project, $"Assets/{assetName}.asset");
            EditorUtility.SetDirty(project);
            AssetDatabase.SaveAssetIfDirty(project);

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(project));
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            string groupName = "LDtkVania";
            var group = settings.FindGroup(groupName);
            if (!group)
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            settings.AddLabel("LDtk Levels");

            AssetReference assetReference = settings.CreateAssetReference(guid);
            assetReference.SetEditorAsset(project);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);

            if (entry != null)
            {
                entry.address = "LDtkVaniaProject";
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
        private LevelsDictionary _levels;

        #endregion

        #region Getters

        public LDtkProjectFile LDtkProjectFile => _ldtkProjectFile;

        public bool SyncLevelsAtCompile => _syncLevelsAtCompile;
        public string ConnectionsContainerName => _connectionsContainerName;
        public string CheckpointsContainerName => _checkpointsContainerName;
        public List<AssetLabelReference> LevelsLabels => _levelsLabels;

        public LevelsDictionary Levels
        {
            get
            {
                _levels ??= new LevelsDictionary();
                return _levels;
            }
        }

        public LdtkJson LDtkProject => _ldtkProjectFile.FromJson;
        public int PixelsPerUnit => LDtkProject.DefaultGridSize;

        #endregion

        #region Handling

        public void Add(MV_Level level)
        {
            if (!Levels.ContainsKey(level.Iid))
            {
                Levels.Add(level.Iid, level);
            }
            else
            {
                Levels[level.Iid] = level;
            }
        }

        public void Remove(MV_Level level)
        {
            if (Levels.ContainsKey(level.Iid))
            {
                Levels.Remove(level.Iid);
            }
        }

        public void Remove(string iid)
        {
            if (!Levels.ContainsKey(iid)) return;
            MV_Level level = Levels[iid];
            Remove(level);
        }

        public void Clear()
        {
            _levels.Clear();
        }

        private void AddToDictionary(MV_Level level)
        {
        }

        private void RemoveFromDictionary(MV_Level level)
        {
        }

        public MV_Level GetLevel(string iid)
        {
            if (!Levels.ContainsKey(iid)) return null;
            return Levels[iid];
        }

        public bool TryGetLevel(string iid, out MV_Level mvLevel)
        {
            return Levels.TryGetValue(iid, out mvLevel);
        }


        public bool HasLevel(string iid)
        {
            return Levels.ContainsKey(iid);
        }

        #endregion

        #region Classes

        [System.Serializable]
        public class LevelsDictionary : SerializedDictionary<string, MV_Level> { }

        #endregion

#if UNITY_EDITOR
        #region Editor Only

        public void SyncLevels()
        {
            // Clear();
            if (!_syncLevelsAtCompile) return;

            HashSet<string> presentLevels = new();
            List<AssetLabelReference> labelReferences = _levelsLabels;

            foreach (AssetLabelReference labelReference in labelReferences)
            {
                AsyncOperationHandle handle = Addressables.LoadResourceLocationsAsync(labelReference.labelString);
                handle.WaitForCompletion();

                if (handle.Status != AsyncOperationStatus.Succeeded) continue;

                IList<IResourceLocation> locations = (IList<IResourceLocation>)handle.Result;

                foreach (IResourceLocation location in locations)
                {
                    string levelIid = HandleLevelLocation(location);
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
                Remove(iid);
            }
        }

        private string HandleLevelLocation(IResourceLocation location)
        {
            LDtkComponentLevel componentLevel = AssetDatabase.LoadAssetAtPath<LDtkComponentLevel>(location.InternalId);
            if (componentLevel == null) return null;

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(location.InternalId);
            if (asset == null) return null;

            LDtkLevelFile file = AssetDatabase.LoadAssetAtPath<LDtkLevelFile>(location.InternalId);
            if (file == null) return null;

            Level ldtkLevel = file.FromJson;
            if (TryGetLevel(ldtkLevel.Iid, out MV_Level mvLevel))
            {
                mvLevel.UpdateInfo(componentLevel, location, asset, file);
                return mvLevel.Iid;
            }

            mvLevel = new(componentLevel, location, asset, file);
            Add(mvLevel);

            return mvLevel.Iid;
        }

        #endregion
#endif
    }
}