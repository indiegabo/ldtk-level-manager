using System;
using Cinemachine;
using LDtkUnity;
using LDtkVania.Utils;
using UnityEngine;

namespace LDtkVania
{
    [RequireComponent(typeof(LevelBehaviour))]
    public class LevelBoundaries : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private PolygonCollider2D _boundaries;

        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;

        [SerializeField]
        private LevelBoundariesUpdater _boundariesUpdater;

        [SerializeField]
        private GameObjectProvider _mainCharacterProvider;

        #endregion

        #region Fields

        private LevelBehaviour _levelBehaviour;
        private LDtkComponentLevel _ldtkComponentLevel;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _levelBehaviour = GetComponent<LevelBehaviour>();
            Compose();
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
        private void Start()
        {
        }

        #endregion

        #region Preparing

        public void Compose()
        {
            _ldtkComponentLevel = GetComponent<LDtkComponentLevel>();
            Vector2 size = _ldtkComponentLevel.Size;
            _boundaries.points = new Vector2[] {
                new(size.x, size.y),
                new(0, size.y),
                new(0, 0),
                new(size.x, 0)
            };
        }

        #endregion

        private void OnLevelExited(LevelBehaviour arg0)
        {
        }

        private void OnLevelPreparationStarted(LevelBehaviour arg0, Vector2 arg1)
        {
            if (_mainCharacterProvider.TryGet(out GameObject characterObj))
            {
                _virtualCamera.Follow = characterObj.transform;
            }
            _virtualCamera.gameObject.SetActive(true);
        }
    }
}