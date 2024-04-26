using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDtkUnity;
using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    public class MV_LevelConnection : MonoBehaviour, MV_IConnection
    {
        #region Inspector

        [SerializeField]
        private BoxCollider2D _collider2D;

        private string _playerTag;

        [SerializeField]
        private List<string> _transitionsTargets;

        [SerializeField]
        private float _colliderWidth;

        [SerializeField]
        private float _colliderHeight;

        [SerializeField]
        private float _colliderVerticalOffset;

        [SerializeField]
        private float _colliderHorizontalOffset;

        [SerializeField]
        private UnityEvent _used;

        #endregion

        #region Fields

        private LDtkFields _fields;
        private bool _active;
        private bool _transitioning;

        private string _key;
        private string _targetLevelIid;
        private Vector2 _spawnPosition;
        private int _directionSign;

        #endregion

        #region Getters

        public UnityEvent Used => _used;

        string MV_IConnection.TargetLevelIid => _targetLevelIid;
        string MV_IConnection.Key => _key;
        Vector2 MV_IConnection.SpawnPosition => _spawnPosition;
        public int DirectionSign => _directionSign;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _fields = GetComponent<LDtkFields>();
            Setup(_fields);
        }

        #endregion

        #region Activation

        void MV_IConnection.Activate()
        {
            gameObject.SetActive(true);
            _active = true;
        }

        void MV_IConnection.Deactivate()
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

            // _transitionsTargets = fields.GetStringArray(_globalTransitionsTargetsKey).ToList();

            // _colliderWidth = fields.GetFloat(_colliderWidthKey);
            // _colliderHeight = fields.GetFloat(_colliderHeightKey);

            // _colliderVerticalOffset = fields.GetFloat(_colliderVerticalOffsetKey);
            // _colliderHorizontalOffset = fields.GetFloat(_colliderHorizontalOffsetKey);

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

        private async Task TriggerLevelTransition()
        {
            _transitioning = true;
            await MV_LevelTransitioner.Instance.TransitionInto(this);
            _transitioning = false;
        }

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            if (!_active || _transitioning || !otherCollider.gameObject.CompareTag(_playerTag)) return;
            _used.Invoke();
            _ = TriggerLevelTransition();
        }

        #endregion
    }
}