using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDtkUnity;
using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    public class MV_LevelConnection : MonoBehaviour, IConnection
    {
        #region Inspector

        [SerializeField]
        private string _playerTag;

        [SerializeField]
        private UnityEvent _used;

        #endregion

        #region Fields

        private LDtkIid _ldtkIid;
        private LDtkFields _fields;
        private MV_LevelAnchor _LevelAnchor;
        private BoxCollider2D _collider2D;

        private bool _active;
        private bool _transitioning;

        private string _targetLevelIid;
        private string _targetConnectionIid;

        private float _colliderWidth;
        private float _colliderHeight;
        private float _colliderVerticalOffset;
        private float _colliderHorizontalOffset;

        #endregion

        #region Getters

        public UnityEvent Used => _used;

        #endregion

        #region Behaviour

        #endregion

        #region IConnection

        public string Iid
        {
            get
            {
                if (_ldtkIid == null)
                {
                    _ldtkIid = GetComponent<LDtkIid>();
                }
                return _ldtkIid.Iid;
            }
        }

        string IConnection.TargetIid => _targetConnectionIid;

        ILevelAnchor IConnection.Anchor
        {
            get
            {
                if (_LevelAnchor == null)
                {
                    LDtkReferenceToAnEntityInstance anchorRef = _fields.GetEntityReference("Anchor");
                    _LevelAnchor = anchorRef.GetEntity().GetComponent<MV_LevelAnchor>();
                }
                return _LevelAnchor;
            }
        }

        void IConnection.Initialize()
        {
            _collider2D = GetComponent<BoxCollider2D>();
            _ldtkIid = GetComponent<LDtkIid>();
            _fields = GetComponent<LDtkFields>();

            LDtkReferenceToAnEntityInstance entityRef = _fields.GetEntityReference("TargetConnection");
            _targetConnectionIid = entityRef.EntityIid;
            _targetLevelIid = entityRef.LevelIid;

            _colliderWidth = _fields.GetFloat("Width");
            _colliderHeight = _fields.GetFloat("Height");

            _colliderVerticalOffset = _fields.GetFloat("VerticalOffset");
            _colliderHorizontalOffset = _fields.GetFloat("HorizontalOffset");

            SetupPosition();
            SetupCollider();
        }

        void IConnection.SetActive(bool isActive)
        {
            _active = true;
            gameObject.SetActive(isActive);
        }

        #endregion

        #region Position

        private void SetupPosition()
        {
            Vector2 position = new()
            {
                x = Mathf.Floor(transform.position.x) + 0.5f,
                y = Mathf.Floor(transform.position.y)
            };

            // Centralizing case the collider size is odd
            if (_colliderHeight % 2 != 0)
            {
                position.y += 0.5f;
            }

            transform.position = position;
        }

        #endregion

        #region Collisions

        private void SetupCollider()
        {
            _collider2D.isTrigger = true;

            SetColliderSize();
            SetColliderOffset();
        }

        private void SetColliderOffset()
        {
            _collider2D.offset = new Vector2(_colliderHorizontalOffset, _colliderVerticalOffset);
        }

        private void SetColliderSize()
        {
            _collider2D.size = new Vector2(_colliderWidth, _colliderHeight);
        }

        private async Task TransitionTask()
        {
            _transitioning = true;
            await MV_LevelTransitioner.Instance.TransitionInto(_targetLevelIid, this);
            _transitioning = false;
        }

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            if (!_active || _transitioning || !otherCollider.gameObject.CompareTag(_playerTag)) return;
            _used.Invoke();
            _ = TransitionTask();
        }

        #endregion
    }
}