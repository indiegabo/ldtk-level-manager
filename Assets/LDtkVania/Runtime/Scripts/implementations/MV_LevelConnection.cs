using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDtkUnity;
using Sirenix.OdinInspector;
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

        [Button]
        public void TriggerLevelTransition()
        {
            _ = TransitionTask();
        }

        #endregion

        #region Fields

        private LDtkFields _fields;
        private BoxCollider2D _collider2D;

        private bool _active;
        private bool _transitioning;

        private string _key;
        private string _targetLevelIid;
        private Vector2 _spawnPosition;
        private int _directionSign;


        private float _colliderWidth;
        private float _colliderHeight;
        private float _colliderVerticalOffset;
        private float _colliderHorizontalOffset;

        #endregion

        #region Getters

        public UnityEvent Used => _used;
        public LDtkFields Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = GetComponent<LDtkFields>();
                    Setup(_fields);
                }
                return _fields;
            }
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
        }

        #endregion

        #region IConnection

        string IConnection.TargetLevelIid => _targetLevelIid;
        string IConnection.Key => _key;
        Vector2 IConnection.SpawnPoint => _spawnPosition;
        public int FacingSign => _directionSign;

        void IConnection.Initialize()
        {
            _collider2D = GetComponent<BoxCollider2D>();
            _fields = GetComponent<LDtkFields>();
            Setup(_fields);
        }

        void IConnection.Activate()
        {
            gameObject.SetActive(true);
            _active = true;
        }

        void IConnection.Deactivate()
        {
            _active = false;
            gameObject.SetActive(false);
        }


        #endregion

        #region Setup

        private void Setup(LDtkFields fields)
        {
            _targetLevelIid = fields.GetString("TargetLevelIid");
            _key = fields.GetString("ConnectionKey");
            _spawnPosition = fields.GetPoint("SpawnPosition");
            _directionSign = fields.GetInt("DirectionSign");

            _colliderWidth = fields.GetFloat("ColliderWidth");
            _colliderHeight = fields.GetFloat("ColliderHeight");

            _colliderVerticalOffset = fields.GetFloat("ColliderVerticalOffset");
            _colliderHorizontalOffset = fields.GetFloat("ColliderHorizontalOffset");

            SetupPosition();
            SetupCollider();
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

            SetiColliderSize();
            SetColliderOffset();
        }

        private void SetColliderOffset()
        {
            _collider2D.offset = new Vector2(_colliderHorizontalOffset, _colliderVerticalOffset);
        }

        private void SetiColliderSize()
        {
            _collider2D.size = new Vector2(_colliderWidth, _colliderHeight);
        }

        private async Task TransitionTask()
        {
            _transitioning = true;
            await MV_LevelTransitioner.Instance.TransitionInto(this);
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