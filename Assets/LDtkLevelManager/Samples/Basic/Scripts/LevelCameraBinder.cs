using LDtkUnity;
using LDtkLevelManager.Utils;
using UnityEngine;
using Cinemachine;

namespace LDtkLevelManager.Implementations.Basic
{
    [RequireComponent(typeof(UniverseLevelBehaviour))]
    [RequireComponent(typeof(LevelBoundaries))]
    public class LevelCameraBinder : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;

        #endregion

        #region Fields

        private UniverseLevelBehaviour _levelBehaviour;
        private LevelBoundaries _boundaries;
        private LDtkComponentLevel _ldtkComponentLevel;

        private CinemachineConfiner2D _confiner;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _levelBehaviour = GetComponent<UniverseLevelBehaviour>();
            _boundaries = GetComponent<LevelBoundaries>();
            _confiner = _virtualCamera.gameObject.GetComponent<CinemachineConfiner2D>();
            _virtualCamera.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _levelBehaviour.Deactivated.AddListener(OnLevelExited);
            _levelBehaviour.PreparationStarted.AddListener(OnLevelPreparationStarted);
        }

        private void OnDisable()
        {
            _levelBehaviour.Deactivated.RemoveListener(OnLevelExited);
            _levelBehaviour.PreparationStarted.RemoveListener(OnLevelPreparationStarted);
        }

        #endregion

        #region Level Events

        private void OnLevelExited(UniverseLevelBehaviour arg0)
        {
        }

        private void OnLevelPreparationStarted(UniverseLevelBehaviour levelBehaviour, ILevelFlowSubject subject, Vector2 position)
        {
            _confiner.m_BoundingShape2D = _boundaries.Shape;

            _virtualCamera.Follow = subject.transform;

            _virtualCamera.gameObject.SetActive(true);
        }

        #endregion
    }
}