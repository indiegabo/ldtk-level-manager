using UnityEngine;
using LDtkLevelManager.Cartography;
using LDtkLevelManager.Utils;
using LDtkLevelManager.Implementations.Basic;

namespace Tests
{
    public class WorldDrawer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private MapLevelDrawer _mapLevelDrawerPrefab;

        [SerializeField]
        private GameObject _characterPinPrefab;

        #endregion

        #region Fields

        private Player _player;
        private Cartographer _cartographer;
        private SpriteRenderer _renderer;
        private Transform _characterPinTransform;
        private float _scaledOffsetY;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _cartographer = GetComponent<Cartographer>();
            _renderer = GetComponent<SpriteRenderer>();

            _characterPinTransform = Instantiate(_characterPinPrefab).transform;

            _scaledOffsetY = 0.75f * _cartographer.ScaleFactor;
        }

        private void OnEnable()
        {
            EvaluatePlayer();
        }

        private void OnDisable()
        {

        }

        private void Start()
        {
            if (!_cartographer.TryWorldCartography("City", out WorldCartography worldCartography)) return;

            Vector2 worldCenter = worldCartography.Bounds.ScaledCenter;
            transform.position = new Vector3(worldCenter.x, worldCenter.y, transform.position.z);

            foreach (AreaCartography area in worldCartography.GetAllAreas())
            {
                foreach (LevelCartography levelCartography in area.GetAllLevels())
                {
                    MapLevelDrawer mapLevelDrawer = Instantiate(_mapLevelDrawerPrefab, transform);
                    mapLevelDrawer.Initialize(levelCartography, transform.position.z);
                }
            }

            _characterPinTransform.localScale *= _cartographer.ScaleFactor;
        }

        private void Update()
        {
            if (_player == null) return;
            Vector2 newPos = _cartographer.CalculateScaledPosition(_player.transform.position);
            _characterPinTransform.position = new Vector3(newPos.x, newPos.y + _scaledOffsetY, transform.position.z);
        }

        #endregion

        #region Player

        private void EvaluatePlayer()
        {

        }

        private void OnPlayerRegistered(GameObject playerGO)
        {
            _player = playerGO.GetComponent<Player>();
        }

        #endregion
    }
}