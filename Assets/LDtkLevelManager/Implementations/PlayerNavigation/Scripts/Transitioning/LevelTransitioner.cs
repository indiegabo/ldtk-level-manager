using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

# if CINEMACHINE_UNDER_3
using Cinemachine;
# else
using Unity.Cinemachine;
# endif

namespace LDtkLevelManager.PlayerNavigation
{
    public class LevelTransitioner : MonoBehaviour
    {
        #region Inspector        

        [SerializeField]
        private Canvas _curtainsCanvas;

        [SerializeField]
        private Animator _curtainsPrefab;

        [SerializeField]
        private LevelTransitionBridge _transitionBridge;

        [SerializeField]
        private UnityEvent _transitionStartedEvent;

        [SerializeField]
        private UnityEvent _transitionEndedEvent;

        #endregion

        #region Fields

        private bool _transitioning = false;
        private Animator _curtainsAnimator;

        #endregion

        #region Getters

        public bool Transitioning => _transitioning;
        public UnityEvent TransitionStartedEvent => _transitionStartedEvent;
        public UnityEvent TransitionEndedEvent => _transitionEndedEvent;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _curtainsAnimator = Instantiate(_curtainsPrefab, _curtainsCanvas.transform);
            Image curtainsImage = _curtainsAnimator.GetComponent<Image>();
            curtainsImage.color = new Color(0, 0, 0, 0);
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

        public void TransitionIntoSpot(string levelIid, string spotIid)
        {
            _ = TransitionIntoAwaitable(levelIid, spotIid);
        }

        public void TransitionToConnection(string levelIid, IConnection connection)
        {
            _ = TransitionIntoAwaitable(levelIid, connection);
        }

        public void TransitionToPortal(string levelIid, IPortal portal)
        {
            _ = TransitionToPortalAwaitable(levelIid, portal);
        }


        public async UniTask TransitionIntoAwaitable(string levelIid, string spotIid)
        {
            LevelLoader.Instance.Exit();
            await BeforePreparationTask();

#if CINEMACHINE_UNDER_3
            CinemachineVirtualCamera camera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineVirtualCamera;            
            await LevelLoader.Instance.LoadLevel(levelIid);
#else
            CinemachineCamera camera = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera as CinemachineCamera;
#endif

            await LevelLoader.Instance.LoadLevel(levelIid);

            if (camera != null)
            {
                camera.Follow = null;
                camera.gameObject.SetActive(false);
            }
            LevelLoader.Instance.Prepare(levelIid, spotIid);
            await AfterPreparationTask();
        }

        public async UniTask TransitionIntoAwaitable(string levelIid, IConnection connection)
        {
            LevelLoader.Instance.Exit();
            await BeforePreparationTask();
#if CINEMACHINE_UNDER_3
            CinemachineVirtualCamera camera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineVirtualCamera;            
            await LevelLoader.Instance.LoadLevel(levelIid);
#else
            CinemachineCamera camera = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera as CinemachineCamera;
#endif
            await LevelLoader.Instance.LoadLevel(levelIid);

            if (camera != null)
            {
                camera.Follow = null;
                camera.gameObject.SetActive(false);
            }

            LevelLoader.Instance.Prepare(levelIid, connection);
            await AfterPreparationTask();
        }

        public async UniTask TransitionToPortalAwaitable(string levelIid, IPortal portal)
        {
            _transitioning = true;
            _transitionStartedEvent.Invoke();

            LevelLoader.Instance.Exit();
            await CloseCurtains();

            await LevelLoader.Instance.LoadLevel(levelIid);
            LevelLoader.Instance.Prepare(levelIid, portal);

            await OpenCurtains();
            LevelLoader.Instance.Enter();

            _transitioning = false;
            _transitionEndedEvent.Invoke();
        }

        private async UniTask BeforePreparationTask()
        {
            _transitioning = true;
            _transitionStartedEvent.Invoke();

            // Closing curtains
            await PerformTransitions(LevelTransitionMoment.Close);

            // Must be after closing curtains because of camera blend
            LevelLoader.Instance.Exit();
        }

        private async UniTask AfterPreparationTask()
        {
            // Opening curtains
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            await PerformTransitions(LevelTransitionMoment.Open);

            if (Camera.main.TryGetComponent<CinemachineBrain>(out var cinemachineBrain))
            {
                await WaitOnCameraBlend(cinemachineBrain);
            }

            // "Activating" level
            LevelLoader.Instance.Enter();

            _transitioning = false;
            _transitionEndedEvent.Invoke();
        }

        private async UniTask PerformTransitions(LevelTransitionMoment moment)
        {
            // For now this is just a dummy transition
            await UniTask.CompletedTask;
        }

        #endregion
        private async UniTask WaitOnCameraBlend(CinemachineBrain brain)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            var blend = brain.ActiveBlend;
            if (blend == null) return;

            float delay = blend.Duration > 0.5f ? blend.Duration - 0.5f : 0f;
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
        }

        private async UniTask CloseCurtains()
        {
            _curtainsAnimator.Play("CurtainsClose");
            int length = _curtainsAnimator.GetCurrentAnimatorClipInfo(0).Length;
            await UniTask.Delay(TimeSpan.FromSeconds(length));
        }

        private async UniTask OpenCurtains()
        {
            _curtainsAnimator.Play("CurtainsOpen");
            int length = _curtainsAnimator.GetCurrentAnimatorClipInfo(0).Length;
            await UniTask.Delay(TimeSpan.FromSeconds(length));

            if (Camera.main.TryGetComponent<CinemachineBrain>(out var cinemachineBrain))
            {
                await WaitOnCameraBlend(cinemachineBrain);
            }
        }
    }
}