using LDtkUnity;
using LDtkLevelManager.Utils;
using UnityEngine;

# if CINEMACHINE_UNDER_3
using Cinemachine;
# else
using Unity.Cinemachine;
# endif

namespace LDtkLevelManager.PlayerNavigation
{
    [RequireComponent(typeof(LevelBehaviour))]
    [RequireComponent(typeof(LevelBoundaries))]
    public class LevelCameraBinder : MonoBehaviour
    {
        #region Inspector

# if CINEMACHINE_UNDER_3
        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;
# else
        [SerializeField]
        private CinemachineCamera _virtualCamera;
# endif

        [SerializeField]
        private GameObjectProvider _mainCharacterProvider;

        #endregion

        #region Fields

        private LevelBehaviour _levelBehaviour;
        private LevelBoundaries _boundaries;
        private LDtkComponentLevel _ldtkComponentLevel;

        private CinemachineConfiner2D _confiner;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _levelBehaviour = GetComponent<LevelBehaviour>();
            _boundaries = GetComponent<LevelBoundaries>();
            _confiner = _virtualCamera.gameObject.GetComponent<CinemachineConfiner2D>();
            _virtualCamera.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _levelBehaviour.ExitedEvent.AddListener(OnLevelExited);
            _levelBehaviour.PreparationStartedEvent.AddListener(OnLevelPreparationStarted);
        }

        private void OnDisable()
        {
            _levelBehaviour.ExitedEvent.RemoveListener(OnLevelExited);
            _levelBehaviour.PreparationStartedEvent.RemoveListener(OnLevelPreparationStarted);
        }

        #endregion

        #region Level Events

        private void OnLevelExited(LevelBehaviour arg0)
        {
        }

        private void OnLevelPreparationStarted(LevelBehaviour arg0, Vector2 arg1)
        {

#if CINEMACHINE_UNDER_3
            _confiner.m_BoundingShape2D = _boundaries.Shape;
#else
            _confiner.BoundingShape2D = _boundaries.Shape;
# endif

            if (_mainCharacterProvider.TryGet(out GameObject characterObj))
            {
                _virtualCamera.Follow = characterObj.transform;
            }

            _virtualCamera.gameObject.SetActive(true);
        }

        #endregion
    }
}