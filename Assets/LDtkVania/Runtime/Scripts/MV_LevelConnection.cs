using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDtkUnity;
using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    public class MV_LevelConnection : MonoBehaviour, ILDtkImportedFields
    {
        #region Inspector

        [SerializeField]
        private MV_LevelTransitioner _transitioner;

        [SerializeField]
        private BoxCollider2D _collider2D;

        private string _playerTag;

        [SerializeField]
        private string _targetLevelKey;

        [SerializeField]
        private string _spawnPositionKey;

        [SerializeField]
        private string _connectionKeyKey;

        [SerializeField]
        private string _globalTransitionsTargetsKey;

        [SerializeField]
        private string _directionSignKey;

        [SerializeField]
        private string _colliderWidthKey;

        [SerializeField]
        private string _colliderHeightKey;

        [SerializeField]
        private string _colliderVerticalOffsetKey;

        [SerializeField]
        private string _colliderHorizontalOffsetKey;

        [SerializeField]
        private string _targetLevelIid;

        [SerializeField]
        private MV_LevelTrail _trail;

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

        private void TriggerSetup()
        {
            Setup(Fields);
        }

        [SerializeField]
        private UnityEvent _used;

        #endregion

        #region Fields

        private LDtkFields _fields;
        private bool _active;
        private bool _transitioning;

        #endregion

        #region Getters

        public Vector2 EnterSpawnPosition => _trail.SpawnPosition;
        public int DirectionSign => _trail.DirectionSign;

        public MV_LevelTrail Trail => _trail;

        public LDtkFields Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = GetComponent<LDtkFields>();
                    if (_fields == null)
                    {
                        MV_Logger.Error("Could not find LDtkFields component", this);
                    }
                };
                return _fields;
            }
        }

        public UnityEvent Used => _used;

        #endregion

        #region Behaviour

        private void Awake()
        {
            Setup(Fields);
        }

        #endregion

        #region LDtk Fields

        public void OnLDtkImportFields(LDtkFields fields)
        {
            Setup(fields);
        }

        #endregion

        #region Activation

        public void Activate()
        {
            gameObject.SetActive(true);
            _active = true;
        }

        public void Deactivate()
        {
            _active = false;
            gameObject.SetActive(false);
        }


        #endregion

        #region Setup

        private void Setup(LDtkFields fields)
        {
            _targetLevelIid = fields.GetString(_targetLevelKey);

            _trail = new MV_LevelTrail
            {
                ConnectionKey = fields.GetString(_connectionKeyKey),
                SpawnPosition = fields.GetPoint(_spawnPositionKey).CenterInUnit(),
                DirectionSign = fields.GetInt(_directionSignKey)
            };

            _transitionsTargets = fields.GetStringArray(_globalTransitionsTargetsKey).ToList();

            _colliderWidth = fields.GetFloat(_colliderWidthKey);
            _colliderHeight = fields.GetFloat(_colliderHeightKey);

            _colliderVerticalOffset = fields.GetFloat(_colliderVerticalOffsetKey);
            _colliderHorizontalOffset = fields.GetFloat(_colliderHorizontalOffsetKey);

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
            await _transitioner.TransitionIntoLevel(_targetLevelIid, _trail, _transitionsTargets);
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