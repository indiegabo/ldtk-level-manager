using LDtkUnity;
using UnityEngine;

namespace LDtkVania
{
    public class MV_LevelSetuper : MonoBehaviour, ILDtkImportedLevel
    {
        #region Inspector

        [SerializeField]
        private string _ldtkLevelIid;

        [SerializeField]
        private PolygonCollider2D _cameraConfinerCollider;


        [SerializeField]
        private void TriggerPositioning()
        {
            if (!MV_Project.Instance.TryGetLevel(_ldtkLevelIid, out MV_Level mvLevel))
            {
                MV_Logger.Error($"{name} could not find level {_ldtkLevelIid}", this);
                return;
            }
            Level ldtkLevel = mvLevel.LDtkLevel;
            PostitionLevel(ldtkLevel);
        }

        [SerializeField]
        private void TriggerFullSetup()
        {
            if (!MV_Project.Instance.TryGetLevel(_ldtkLevelIid, out MV_Level mvLevel))
            {
                MV_Logger.Error($"{name} could not find level {_ldtkLevelIid}", this);
                return;
            }

            Level ldtkLevel = mvLevel.LDtkLevel;
            Setup(ldtkLevel);
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            if (!MV_Project.Instance.TryGetLevel(_ldtkLevelIid, out MV_Level mvLevel))
            {
                MV_Logger.Error($"{name} could not find level {_ldtkLevelIid}", this);
                return;
            }
            Setup(mvLevel.LevelFile.FromJson);
        }

        #endregion

        #region Setup

        private void Setup(Level ldtkLevel)
        {
            PostitionLevel(ldtkLevel);
            SetupCameraConfiningShape(ldtkLevel);
        }

        #endregion

        #region Positioning

        private void PostitionLevel(Level ldtkLevel)
        {
            Vector2 position = ldtkLevel.UnityWorldSpaceCoord(WorldLayout.GridVania, MV_Project.Instance.PixelsPerUnit);
            transform.position = position;
        }

        #endregion

        #region Cameras

        private void SetupCameraConfiningShape(Level ldtkLevel)
        {
            if (_cameraConfinerCollider == null) return;

            _cameraConfinerCollider.gameObject.name = $"{name} - Camera Confiner";

            Rect bounds = ldtkLevel.UnityWorldSpaceBounds(WorldLayout.GridVania, MV_Project.Instance.PixelsPerUnit);
            Vector2 size = bounds.size;
            _cameraConfinerCollider.ShapeFromSize(size);
        }

        #endregion

        #region LDtk   

        public void OnLDtkImportLevel(Level ldtkLevel)
        {
            _ldtkLevelIid = ldtkLevel.Iid;
            Setup(ldtkLevel);
        }

        #endregion
    }
}