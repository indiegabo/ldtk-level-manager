using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using LDtkUnity;

namespace LDtkVania
{
    [DefaultExecutionOrder(-1000000)]
    public class MV_LevelManager : MonoBehaviour
    {
        #region Static

        private static MV_LevelManager _instance;
        public static MV_LevelManager Instance => _instance;

        #endregion

        #region Inspector        

        [Tooltip("Mark this if you want this object to NOT be destroyed whe a new scene is loaded.")]
        [SerializeField]
        private bool _persistent = true;

        [SerializeField]
        private bool _alertAboutOtherInstances;

        [SerializeField]
        private MV_Project _project;

        [SerializeField]
        [Min(1)]
        private int _depth = 1;

        [SerializeField]
        private UnityEvent<MV_Level> _levelPreparedEvent;

        [SerializeField]
        private UnityEvent<MV_Level> _levelEnteredEvent;

        [SerializeField]
        private UnityEvent<MV_Level> _levelExitedEvent;

        #endregion

        #region Fields

        private MV_Level _currentLevel;
        private MV_LevelBehaviour _currentBehaviour;

        private readonly Dictionary<string, MV_LevelBehaviour> _registeredBehaviours = new();
        private readonly Dictionary<string, GameObject> _loadedObjects = new();
        private readonly Dictionary<string, SceneInstance> _loadedScenes = new();
        private readonly HashSet<string> _shouldBeLoaded = new();
        private readonly HashSet<string> _shouldBeUnloaded = new();

        #endregion

        #region Getters

        public string ConnectionsContainerName => _project.ConnectionsContainerName;
        public string CheckpointsContainerName => _project.CheckpointsContainerName;

        public UnityEvent<MV_Level> LevelPreparedEvent => _levelPreparedEvent;
        public UnityEvent<MV_Level> LevelEnteredEvent => _levelEnteredEvent;
        public UnityEvent<MV_Level> LevelDeactivatedEvent => _levelExitedEvent;

        #endregion

        #region Behaviour

        public void Awake()
        {
            LDtkIidBank.CacheIidData(_project.LDtkProject);

            MV_LevelManager currentInstance = Instance;

            if (currentInstance != null && currentInstance != this)
            {
                if (_alertAboutOtherInstances)
                {
                    MV_Logger.Error($"{name} - Awake interrupted due to other instance being already active.", this);
                }

                Destroy(gameObject);

                return;
            }

            _instance = this;

            if (_persistent)
                DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Entering Levels

        /// <summary>
        /// This should be used when the target level has no neighbours already loaded 
        /// and therefore needs to await all of them being loaded before 
        /// activating the current level.
        /// 
        /// This also unloads all loaded levels before loading the target level and 
        /// its neighbours
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public async Task FullLevelLoad(string iid, MV_LevelLoadMode mode = MV_LevelLoadMode.LoadOnly)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Level under LDtk Iid {iid} not found while trying a full load", this);
                return;
            }

            ExitLevel();

            await UnloadAllAsync();

            _registeredBehaviours.Clear();
            _loadedObjects.Clear();
            _loadedScenes.Clear();

            await LoadNeighboursAsync(level);

            if (!mode.Equals(MV_LevelLoadMode.LoadAndEnter))
            {
                return;
            };

            _currentLevel = level;
            _currentBehaviour = _registeredBehaviours[_currentLevel.Iid];
            _currentBehaviour.Prepare();

            EnterLevel();
        }

        /// <summary>
        /// This should be used while transitioning from one level to another.
        /// Have in mind that the target level should be already registered.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public void Prepare(MV_Level level)
        {
            if (string.IsNullOrEmpty(level.Iid))
            {
                MV_Logger.Error($"Null or Empty level Iid.", this);
                return;
            }

            if (!_registeredBehaviours.ContainsKey(level.Iid))
            {
                MV_Logger.Error($"Trying to prepare a non registered level.", this);
                return;
            }

            _currentLevel = level;
            _currentBehaviour = _registeredBehaviours[_currentLevel.Iid];
            _ = LoadNeighboursAsync(_currentLevel);

            _currentBehaviour.Prepare();
        }

        /// <summary>
        /// This should be used while transitioning from one level to another.
        /// Have in mind that the target level should be already loaded.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public void PrepareLevel(string iid)
        {
            if (!SetLevelForPreparation(iid, out MV_LevelBehaviour levelBehaviour))
            {
                return;
            }
            levelBehaviour.Prepare();
        }

        /// <summary>
        /// This should be used while transitioning from one level to another.
        /// Have in mind that the target level should be already loaded.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public void PrepareLevel(ILevelAnchor checkpoint)
        {
            if (!SetLevelForPreparation(checkpoint.LevelIId, out MV_LevelBehaviour levelBehaviour))
            {
                return;
            }
            levelBehaviour.Prepare(checkpoint);
        }

        public void PrepareLevel(IConnection connection)
        {
            if (!SetLevelForPreparation(connection.TargetLevelIid, out MV_LevelBehaviour levelBehaviour))
            {
                return;
            }
            levelBehaviour.Prepare(connection);
        }

        private bool SetLevelForPreparation(string iid, out MV_LevelBehaviour behaviour)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Trying to prepare a level by Iid {iid} but it was not found.", this);
                behaviour = null;
                return false;
            }

            if (!_registeredBehaviours.TryGetValue(iid, out behaviour))
            {
                MV_Logger.Error($"Trying to prepare a level by Iid {iid} but it is not registered.", this);
                return false;
            }

            _currentLevel = level;
            _currentBehaviour = behaviour;
            _ = LoadNeighboursAsync(_currentLevel);

            return true;
        }

        public void EnterLevel()
        {
            if (_currentLevel == null || _currentBehaviour == null)
            {
                MV_Logger.Error($"Trying to enter a non prepared level.", this);
                return;
            }

            _currentBehaviour.Enter();
            _levelEnteredEvent.Invoke(_currentLevel);
        }

        public void ExitLevel()
        {
            if (_currentBehaviour != null)
            {
                _currentBehaviour.Exit();

                if (_currentLevel != null)
                    _levelExitedEvent.Invoke(_currentLevel);
            }
        }

        #endregion

        #region Loading Neighbours

        private async Task LoadNeighboursAsync(MV_Level level)
        {
            _shouldBeLoaded.Clear();
            _shouldBeUnloaded.Clear();

            _shouldBeLoaded.Add(level.Iid); // Must put current level as first of the list

            for (int i = 0; i < _depth; i++)
            {
                HashSet<string> neighbours = new();

                foreach (string neighbourLevelIid in _shouldBeLoaded)
                {
                    AddNeighboursToList(neighbours, neighbourLevelIid);
                }

                foreach (string neighbourIid in neighbours)
                {
                    if (_shouldBeLoaded.Contains(neighbourIid)) continue;
                    _shouldBeLoaded.Add(neighbourIid);
                }
            }

            foreach (var pair in _loadedObjects)
            {
                if (!_shouldBeLoaded.Contains(pair.Key))
                {
                    _shouldBeUnloaded.Add(pair.Key);
                }
            }

            foreach (var pair in _loadedScenes)
            {
                if (!_shouldBeLoaded.Contains(pair.Key))
                {
                    _shouldBeUnloaded.Add(pair.Key);
                }
            }
            await UnloadMultipleAsync(_shouldBeUnloaded);
            await LoadMultipleAsync(_shouldBeLoaded);
        }

        private void AddNeighboursToList(HashSet<string> list, string iid)
        {
            if (!TryGetLevel(iid, out MV_Level level)) return;
            foreach (LDtkUnity.Level neighbour in level.LDtkLevel.Neighbours)
            {
                if (!TryGetLevel(neighbour.Iid, out MV_Level mvLevel))
                {
                    MV_Logger.Error($"{name} could not find neighbour {neighbour.Iid}", this);
                    continue;
                }

                if (list.Contains(mvLevel.Iid)) continue;
                list.Add(mvLevel.Iid);
            }
        }

        #endregion

        #region Loading Levels

        private async Task LoadMultipleAsync(HashSet<string> levelsToLoad)
        {
            List<Task> levelLoadTasks = new();

            foreach (string toLoadiid in levelsToLoad)
            {
                levelLoadTasks.Add(LoadAsync(toLoadiid));
            }

            await Task.WhenAll(levelLoadTasks);
        }

        private async Task LoadAsync(string iid)
        {
            if (!TryGetLevel(iid, out MV_Level level)) return;

            if (!level.HasScene)
            {
                if (_loadedObjects.ContainsKey(iid)) return;

                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(level.Address);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    MV_Logger.Error($"Async operation for level {level.name} as an object failed.", this);
                    return;
                }

                GameObject loadedObject = Instantiate(handle.Result);
                _loadedObjects.Add(iid, loadedObject);
            }
            else
            {
                if (_loadedScenes.ContainsKey(iid)) return;
                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(level.Scene.AddressableKey, LoadSceneMode.Additive);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    MV_Logger.Error($"Async operation for loading level {level.name} as a scene failed.", this);
                    return;
                }

                _loadedScenes.Add(iid, handle.Result);
            }
        }

        private async Task UnloadAsync(string iid)
        {
            if (!TryGetLevel(iid, out MV_Level level)) return;

            if (_loadedObjects.TryGetValue(iid, out GameObject loadedObject))
            {
                _loadedObjects.Remove(iid);
                Destroy(loadedObject);
            }

            if (_loadedScenes.TryGetValue(iid, out SceneInstance sceneInstance))
            {
                AsyncOperationHandle handle = Addressables.UnloadSceneAsync(sceneInstance, false);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    MV_Logger.Error($"Async operation for unloading level {level.name} as a scene failed.", this);
                    MV_Logger.Warning($"Handle status: {handle.Status}");
                    MV_Logger.Warning($"{handle.OperationException?.StackTrace}");
                    return;
                }

                _loadedScenes.Remove(iid);
            }
        }

        private async Task UnloadMultipleAsync(HashSet<string> levelsToUnload)
        {
            List<Task> unloadTasks = new();

            foreach (string toUnloadIid in levelsToUnload)
            {
                unloadTasks.Add(UnloadAsync(toUnloadIid));
            }

            await Task.WhenAll(unloadTasks);
        }

        private async Task UnloadAllAsync()
        {
            List<Task> tasks = new();

            foreach (var pair in _loadedObjects)
            {
                tasks.Add(UnloadAsync(pair.Key));
            }

            foreach (var pair in _loadedScenes)
            {
                tasks.Add(UnloadAsync(pair.Key));
            }

            await Task.WhenAll(tasks);
        }

        #endregion

        #region Registering Behaviours

        public void RegisterAsBehaviour(string iid, MV_LevelBehaviour behaviour)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Level under LDtk Iid {iid} not found for registering as behaviour", this);
                return;
            }

            if (_registeredBehaviours.ContainsKey(iid))
            {
                MV_Logger.Warning($"Level {level.name}({level.Iid}) already registered as behaviour but is trying to be registered again", this);
                return;
            }

            _registeredBehaviours.Add(iid, behaviour);
        }

        public void UnregisterAsBehaviour(string iid)
        {
            if (!TryGetLevel(iid, out MV_Level level))
            {
                MV_Logger.Error($"Level under LDtk Iid {iid} not found for unregistering as behaviour", this);
                return;
            }

            _registeredBehaviours.Remove(iid);
        }

        #endregion

        #region Providing Levels

        public bool TryGetLevel(string iid, out MV_Level level)
        {
            return _project.TryGetLevel(iid, out level);
        }

        public MV_Level GetLevel(string iid)
        {
            return _project.GetLevel(iid);
        }

        #endregion
    }
}