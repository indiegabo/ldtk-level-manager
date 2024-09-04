using System.Collections.Generic;
using UnityEngine;
using LDtkLevelManager.Utils;

namespace LDtkLevelManager.PlayerNavigation
{
    [CreateAssetMenu(fileName = "Level Transitions Provider", menuName = "Poputara/Gameplay/Levels/Transitions Provider")]
    public class LevelTransitionsProvider : ScriptableObject
    {
        #region Fields

        [SerializeField]
        private LevelTransitionsDictionary _closeTransitions;

        [SerializeField]
        private LevelTransitionsDictionary _openTransitions;

        #endregion

        #region Retrieving

        public List<ITransition> GetTransitions(LevelTransitionMoment moment, string target)
        {
            return moment switch
            {
                LevelTransitionMoment.Close => GetCloseTransitions(target),
                LevelTransitionMoment.Open => GetOpenTransitions(target),
                _ => null,
            };
        }

        private List<ITransition> GetCloseTransitions(string target)
        {
            if (_closeTransitions == null || !_closeTransitions.ContainsKey(target))
                return null;

            return _closeTransitions[target];
        }

        private List<ITransition> GetOpenTransitions(string target)
        {
            if (_openTransitions == null || !_openTransitions.ContainsKey(target))
                return null;

            return _openTransitions[target];
        }

        #endregion

        #region Registering

        public void RegisterTransition(LevelTransitionMoment moment, string target, ITransition transition)
        {
            switch (moment)
            {
                case LevelTransitionMoment.Close:
                    RegisterCloseTransition(target, transition);
                    break;
                case LevelTransitionMoment.Open:
                    RegisterOpenTransition(target, transition);
                    break;
            }
        }

        public void UnregisterTransition(LevelTransitionMoment moment, string target, ITransition transition)
        {
            switch (moment)
            {
                case LevelTransitionMoment.Close:
                    UnregisterCloseTransition(target, transition);
                    break;
                case LevelTransitionMoment.Open:
                    UnregisterOpenTransition(target, transition);
                    break;
            }
        }

        private void RegisterCloseTransition(string target, ITransition transition)
        {
            if (_closeTransitions == null) _closeTransitions = new LevelTransitionsDictionary();

            if (!_closeTransitions.ContainsKey(target))
            {
                _closeTransitions.Add(target, new List<ITransition>());
            }

            List<ITransition> transitions = _closeTransitions[target];

            if (!transitions.Contains(transition))
                transitions.Add(transition);

        }

        private void UnregisterCloseTransition(string target, ITransition transition)
        {
            if (_closeTransitions == null) return;

            if (!_closeTransitions.ContainsKey(target)) return;

            List<ITransition> transitions = _closeTransitions[target];

            transitions.Remove(transition);
        }

        private void RegisterOpenTransition(string target, ITransition transition)
        {
            if (_openTransitions == null) _openTransitions = new LevelTransitionsDictionary();

            if (!_openTransitions.ContainsKey(target))
            {
                _openTransitions.Add(target, new List<ITransition>());
            }

            List<ITransition> transitions = _openTransitions[target];

            if (!transitions.Contains(transition))
                transitions.Add(transition);
        }

        private void UnregisterOpenTransition(string target, ITransition transition)
        {
            if (_openTransitions == null) return;

            if (!_openTransitions.ContainsKey(target)) return;

            List<ITransition> transitions = _openTransitions[target];

            transitions.Remove(transition);
        }

        #endregion

        #region Classes

        public class LevelTransitionsDictionary : SerializedDictionary<string, List<ITransition>> { }

        #endregion
    }
}