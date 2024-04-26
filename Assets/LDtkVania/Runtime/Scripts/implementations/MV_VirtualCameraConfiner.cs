using System;
using Cinemachine;
using UnityEngine;

namespace LDtkVania
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class MV_VirtualCameraConfiner : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private MV_LevelBoundariesUpdater _boundariesUpdater;

        #endregion

        #region Fields

        private CinemachineVirtualCamera _virtualCamera;
        private CinemachineConfiner2D _confiner;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();
            _confiner = GetComponent<CinemachineConfiner2D>();
        }

        private void OnEnable()
        {
            _boundariesUpdater.BoundariesUpdated.AddListener(UpdateConfiner);
        }

        private void OnDisable()
        {
            _boundariesUpdater.BoundariesUpdated.RemoveListener(UpdateConfiner);
        }

        #endregion

        private void UpdateConfiner(PolygonCollider2D bondaries)
        {
            _confiner.m_BoundingShape2D = bondaries;
        }
    }
}