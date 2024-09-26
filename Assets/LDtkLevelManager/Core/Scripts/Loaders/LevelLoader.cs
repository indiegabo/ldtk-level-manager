using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LDtkLevelManager.EventBus;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
    public abstract class LevelLoader : MonoBehaviour
    {
        #region Static

        private static Dictionary<string, LevelLoader> _loaders = new();

        public static LevelLoader For(Project project)
        {
            if (!_loaders.TryGetValue(project.Iid, out LevelLoader loader))
            {
                throw new System.ArgumentException("LevelLoader not found for project: " + project.name);
            }

            return loader;
        }

        public static LevelLoader InstantiateLoader(Project project)
        {
            if (_loaders.ContainsKey(project.Iid))
            {
                throw new System.InvalidOperationException("LevelLoader already exists for project: " + project.name);
            }

            var organization = project.Organization;

            var loader = organization switch
            {
                Project.LevelsOrganization.Unrelated => InstantiateUnrelatedLevelsLoader(),
                Project.LevelsOrganization.Connected => InstantiateConnectedLevelsLoader(project.Strategy),
                _ => throw new System.InvalidOperationException("Unrecognized organization: " + organization),
            };

            loader.Initialize(project);
            _loaders.Add(project.Iid, loader);

            return loader;
        }

        public static void ClearLoaders()
        {
            _loaders.Clear();
        }

        private static LevelLoader InstantiateUnrelatedLevelsLoader()
        {
            return new GameObject().AddComponent<UnrelatedLevelLoader>();
        }

        private static LevelLoader InstantiateConnectedLevelsLoader(Project.ConnectedLoadingStrategy strategy)
        {
            var obj = new GameObject();
            return strategy switch
            {
                Project.ConnectedLoadingStrategy.Neighbours => obj.AddComponent<NeighboursLevelLoader>(),
                Project.ConnectedLoadingStrategy.Worlds => obj.AddComponent<WorldsLevelLoader>(),
                Project.ConnectedLoadingStrategy.Areas => obj.AddComponent<AreasLevelLoader>(),
                _ => throw new System.InvalidOperationException("Unrecognized Loading Strategy: " + strategy)
            };
        }

        #endregion

        #region Fields

        protected Project _project;
        protected LevelInfo _currentLevel;
        protected LevelActivationEvent _activationEventData = new();
        protected LevelDeactivationEvent _deactivationEventData = new();

        #endregion

        #region Initialization

        public void Initialize(Project project)
        {
            if (project == null) throw new System.ArgumentNullException(nameof(project));

            if (!ProjectsService.Instance.TryGetLdtkJson(project, out LdtkJson ldtkProject))
            {
                Logger.Error($"Failed to load LDtkJson for project {project.name}.", this);
                return;
            }

            _project = project;
            gameObject.name = "[LevelLoader] " + _project.name;

            LDtkIidBank.CacheIidData(ldtkProject);
        }

        #endregion


        #region Getters

        /// <summary>
        /// The current project (<see cref="Project"/>).
        /// </summary>   
        public Project Project => _project;

        /// <summary>
        /// The navigation layer's name set in the LDtk project
        /// </summary>
        public string NavigationLayer => _project.NavigationLayer;

        #endregion        

        #region Behaviour

        protected virtual void Awake()
        {
        }

        #endregion

        #region Providing Levels

        /// <summary>
        /// Attempts to retrieve a level by its Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to retrieve.</param>
        /// <param name="level">The retrieved level if successful, or null if not.</param>
        /// <returns>true if the level was successfully retrieved, false otherwise.</returns>
        public virtual bool TryGetLevel(string iid, out LevelInfo level)
        {
            // Try to get the level from the project.
            return _project.TryGetLevel(iid, out level);
        }

        /// <summary>
        /// Retrieves a level by its Iid.
        /// </summary>
        /// <param name="iid">The Iid of the level to retrieve.</param>
        /// <returns>The retrieved level if successful, or null if not.</returns>
        public virtual LevelInfo GetLevel(string iid)
        {
            // Attempt to get the level from the project.
            return _project.GetLevel(iid);
        }

        #endregion

        #region Events

        protected virtual void AnnounceActivation(ConnectedLevelBehaviour behaviour)
        {
            _activationEventData.behaviour = behaviour;
            Bus<LevelActivationEvent>.Raise(_activationEventData);
        }

        protected virtual void AnnounceDeactivation(ConnectedLevelBehaviour behaviour)
        {
            _activationEventData.behaviour = behaviour;
            Bus<LevelDeactivationEvent>.Raise(_deactivationEventData);
        }

        #endregion

        #region Abstractions

        public abstract UniTask LoadLevel(string iid);
        public abstract UniTask LoadLevel(LevelInfo level);

        #endregion

        #region Casting

        public T As<T>() where T : LevelLoader
        {
            return this as T;
        }

        #endregion
    }
}