using LDtkUnity;
using LDtkLevelManager.Utils;
using UnityEngine;
using Cinemachine;

namespace LDtkLevelManager.Implementations.Basic
{
    [RequireComponent(typeof(LevelBehaviour))]
    [RequireComponent(typeof(LevelBoundaries))]
    public class LevelCameraBinder : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;

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
            _confiner.m_BoundingShape2D = _boundaries.Shape;

            if (_mainCharacterProvider.TryGet(out GameObject characterObj))
            {
                _virtualCamera.Follow = characterObj.transform;
            }

            _virtualCamera.gameObject.SetActive(true);
        }

        #endregion
    }
}