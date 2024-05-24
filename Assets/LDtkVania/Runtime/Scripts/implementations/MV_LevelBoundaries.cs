using System;
using Cinemachine;
using LDtkUnity;
using LDtkVania.Utils;
using UnityEngine;

namespace LDtkVania
{
    [RequireComponent(typeof(MV_LevelBehaviour))]
    public class MV_LevelBoundaries : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private PolygonCollider2D _boundaries;

        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;

        [SerializeField]
        private MV_LevelBoundariesUpdater _boundariesUpdater;

        [SerializeField]
        private GameObjectProvider _mainCharacterProvider;

        #endregion

        #region Fields

        private MV_LevelBehaviour _levelBehaviour;
        private LDtkComponentLevel _ldtkComponentLevel;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _levelBehaviour = GetComponent<MV_LevelBehaviour>();
            _ldtkComponentLevel = GetComponent<LDtkComponentLevel>();
            Vector2 size = _ldtkComponentLevel.Size;
            _boundaries.points = new Vector2[] {
                new(size.x, size.y),
                new(0, size.y),
                new(0, 0),
                new(size.x, 0)
            };
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

        private void OnLevelExited(MV_LevelBehaviour arg0)
        {
            _virtualCamera.Follow = null;
            _virtualCamera.gameObject.SetActive(false);
        }

        private void OnLevelPreparationStarted(MV_LevelBehaviour arg0, Vector2 arg1)
        {
            if (_mainCharacterProvider.TryGet(out GameObject characterObj))
            {
                _virtualCamera.Follow = characterObj.transform;
            }
            _virtualCamera.gameObject.SetActive(true);
        }
    }
}