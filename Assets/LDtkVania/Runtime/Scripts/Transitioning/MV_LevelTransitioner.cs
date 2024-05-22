using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using Cinemachine;
using System;

namespace LDtkVania.Transitioning
{
    public class MV_LevelTransitioner : MonoBehaviour
    {
        #region Inspector        

        [SerializeField]
        private MV_LevelTransitionBridge _transitionBridge;

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
        }

        private void OnEnable()
        {
            _transitionBridge.Register(this);
        }

        private void OnDisable()
        {
            _transitionBridge.ClearRegistry();
        }

        #endregion

        #region Transition performing

        public void TransitionInto(
            string levelIid,
            string spotIid
        )
        {
            _ = TransitionIntoAwaitable(levelIid, spotIid);
        }

        public void TransitionInto(
            string levelIid,
            IConnection connection
        )
        {
            _ = TransitionIntoAwaitable(levelIid, connection);
        }

        public async Task TransitionIntoAwaitable(
            string levelIid,
            string spotIid
        )
        {
            await BeforePreparationTask();
            MV_LevelManager.Instance.PrepareLevel(levelIid, spotIid);
            await AfterPreparationTask();
        }

        public async Task TransitionIntoAwaitable(
            string levelIid,
            IConnection connection
        )
        {
            await BeforePreparationTask();
            MV_LevelManager.Instance.PrepareLevel(levelIid, connection);
            await AfterPreparationTask();
        }

        private async Task BeforePreparationTask()
        {
            _transitioning = true;
            _transitionStartedEvent.Invoke();

            // Closing curtains
            await PerformTransitions(LevelTransitionMoment.Close);

            // Must be after closing curtains because of camera blend
            MV_LevelManager.Instance.ExitLevel();
        }

        private async Task AfterPreparationTask()
        {
            // Opening curtains
            await PerformTransitions(LevelTransitionMoment.Open);

            if (Camera.main.TryGetComponent<CinemachineBrain>(out var cinemachineBrain))
            {
                await WaitOnCameraBlend(cinemachineBrain);
            }

            // "Activating" level
            MV_LevelManager.Instance.EnterLevel();

            _transitioning = false;
            _transitionEndedEvent.Invoke();
        }

        private async Task PerformTransitions(LevelTransitionMoment moment)
        {
            // For now this is just a dummy transition
            await Task.CompletedTask;
        }

        #endregion
        private async Task WaitOnCameraBlend(CinemachineBrain brain)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.5f));

            var blend = brain.ActiveBlend;
            if (blend == null) return;

            float delay = blend.Duration > 0.5f ? blend.Duration - 0.5f : 0f;
            await Task.Delay(TimeSpan.FromSeconds(delay));
        }
    }
}