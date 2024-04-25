using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using LDtkVania.Transitioning;

namespace LDtkVania
{
    [CreateAssetMenu(fileName = "Level Transitioner", menuName = "LDtkVania/Level Transitioner")]
    public class MV_LevelTransitioner : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        private MV_PlayerControlBridge _playerControlBridge;

        [SerializeField]
        private LevelTransitionsProvider _globalTransitionsProvider;

        [SerializeField]
        private MV_LevelManager _levelHandler;

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
        public async Task TransitionIntoLevel(
            string levelIid,
            MV_LevelTrail trail,
            List<string> globalTransitionsTargets = null,
            List<ITransition> closeTransitions = null,
            List<ITransition> openTransitions = null
        )
        {
            if (!MV_LevelManager.Instance.TryGetLevel(levelIid, out MV_Level metroidvaniaLevel))
            {

                MV_Logger.Error($"Level {levelIid} not found in level dictionary");
                return;
            }

            _transitioning = true;
            _transitionStartedEvent.Invoke();

            // Removing control from player
            _playerControlBridge.RemoveControl();

            // Closing curtains
            await PerformTransitions(LevelTransitionMoment.Close, globalTransitionsTargets, closeTransitions);

            // Must be after closing curtains because of camera blend
            MV_LevelManager.Instance.ExitLevel();

            // Preparing level
            await MV_LevelManager.Instance.PrepareLevel(metroidvaniaLevel, trail);

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