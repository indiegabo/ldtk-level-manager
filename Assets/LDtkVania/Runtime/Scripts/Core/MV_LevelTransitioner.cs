using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using LDtkVania.Transitioning;

namespace LDtkVania
{
    public class MV_LevelTransitioner : MonoBehaviour
    {
        #region Static

        private static MV_LevelTransitioner _instance;
        public static MV_LevelTransitioner Instance => _instance;

        #endregion

        #region Inspector        

        [Tooltip("Mark this if you want this object to NOT be destroyed whe a new scene is loaded.")]
        [SerializeField]
        private bool _persistent = true;

        [SerializeField]
        private bool _alertAboutOtherInstances;

        [SerializeField]
        private MV_PlayerControlBridge _playerControlBridge;

        [SerializeField]
        private LevelTransitionsProvider _globalTransitionsProvider;

        [SerializeField]
        private GameObjectProvider _mainCharacterProvider;

        [SerializeField]
        private UnityEvent _transitionStartedEvent;

        [SerializeField]
        private UnityEvent _transitionEndedEvent;

        #endregion

        #region Fields

        private bool _transitioning = false;
        private List<Task> _transitionTasks = new();

        #endregion

        #region Getters

        public bool Transitioning => _transitioning;
        public UnityEvent TransitionStartedEvent => _transitionStartedEvent;
        public UnityEvent TransitionEndedEvent => _transitionEndedEvent;

        #endregion

        #region Behaviour

        private void Awake()
        {
            MV_LevelTransitioner currentInstance = Instance;

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

        #region Transition performing

        /// <summary>
        /// Transition into the given Metroidvania Level.
        /// 
        /// </summary>
        /// <param name="levelIid"></param>
        /// <param name="trail"></param>
        /// <param name="globalTransitionsTargets"></param>
        /// <param name="closeTransitions"></param>
        /// <param name="openTransitions"></param>
        /// <returns></returns>
        public async Task TransitionInto(
            string levelIid,
            List<string> globalTransitionsTargets = null,
            List<ITransition> closeTransitions = null,
            List<ITransition> openTransitions = null
        )
        {
            await BeforePreparationTask(globalTransitionsTargets, closeTransitions);
            await MV_LevelManager.Instance.PrepareLevel(levelIid);
            await AfterPreparationTask(globalTransitionsTargets, openTransitions);
        }

        public async Task TransitionInto(
            MV_IConnection connection,
            List<string> globalTransitionsTargets = null,
            List<ITransition> closeTransitions = null,
            List<ITransition> openTransitions = null
        )
        {
            await BeforePreparationTask(globalTransitionsTargets, closeTransitions);
            await MV_LevelManager.Instance.PrepareLevel(connection);
            await AfterPreparationTask(globalTransitionsTargets, openTransitions);
        }

        public async Task TransitionInto(
            MV_ICheckpoint checkpoint,
            List<string> globalTransitionsTargets = null,
            List<ITransition> closeTransitions = null,
            List<ITransition> openTransitions = null
        )
        {
            await BeforePreparationTask(globalTransitionsTargets, closeTransitions);
            await MV_LevelManager.Instance.PrepareLevel(checkpoint);
            await AfterPreparationTask(globalTransitionsTargets, openTransitions);
        }

        private async Task BeforePreparationTask(List<string> globalTransitionsTargets = null, List<ITransition> closeTransitions = null)
        {
            _transitioning = true;
            _transitionStartedEvent.Invoke();

            // Removing control from player
            _playerControlBridge.RemoveControl();

            // Closing curtains
            await PerformTransitions(LevelTransitionMoment.Close, globalTransitionsTargets, closeTransitions);

            // Must be after closing curtains because of camera blend
            MV_LevelManager.Instance.ExitLevel();
        }

        private async Task AfterPreparationTask(List<string> globalTransitionsTargets = null, List<ITransition> openTransitions = null)
        {
            // Opening curtains
            await PerformTransitions(LevelTransitionMoment.Open, globalTransitionsTargets, openTransitions);

            // "Activating" level
            MV_LevelManager.Instance.EnterLevel();

            // Giving back player control
            _playerControlBridge.GiveControl();

            _transitioning = false;
            _transitionEndedEvent.Invoke();
        }

        private async Task PerformTransitions(LevelTransitionMoment moment, List<string> transitionTargets, List<ITransition> transitions)
        {
            if (transitionTargets == null) return;

            if (_transitionTasks == null)
            {
                _transitionTasks = new List<Task>();
            }
            else
            {
                _transitionTasks.Clear();
            }

            foreach (string target in transitionTargets)
            {
                List<ITransition> globalTransitions = _globalTransitionsProvider.GetTransitions(moment, target);

                if (globalTransitions == null) continue;

                foreach (ITransition transition in globalTransitions)
                {
                    _transitionTasks.Add(transition.TransitionInto());
                }
            }

            if (transitions != null)
            {
                foreach (ITransition transition in transitions)
                {
                    _transitionTasks.Add(transition.TransitionInto());
                }
            }

            await Task.WhenAll(_transitionTasks);
        }

        #endregion
    }
}