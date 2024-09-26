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

        private static Dictionary<Project, LevelLoader> _loaders = new();


        public static bool For(Project project, out LevelLoader loader)
        {
            return _loaders.TryGetValue(project, out loader);
        }

        public static void RegisterLoader(Project project, LevelLoader loader)
        {
            if (_loaders.ContainsKey(project))
            {
                _loaders[project] = loader;
                return;
            }

            _loaders.Add(project, loader);
        }

        public static void ClearLoaders()
        {
            _loaders.Clear();
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
            _project = project;

            if (!ProjectsService.Instance.TryGetLdtkJson(_project, out LdtkJson ldtkProject))
            {
                Logger.Error($"Failed to load LDtkJson for project {_project.name}.", this);
                return;
            }

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

        #region Events

        protected virtual void AnnounceActivation(UniverseLevelBehaviour behaviour)
        {
            _activationEventData.behaviour = behaviour;
            Bus<LevelActivationEvent>.Raise(_activationEventData);
        }

        protected virtual void AnnounceDeactivation(UniverseLevelBehaviour behaviour)
        {
            _activationEventData.behaviour = behaviour;
            Bus<LevelDeactivationEvent>.Raise(_deactivationEventData);
        }

        #endregion

        #region Abstractions

        public abstract UniTask LoadLevel(string iid);
        public abstract UniTask LoadLevel(LevelInfo level);

        #endregion
    }
}