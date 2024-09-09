using Cysharp.Threading.Tasks;
using LDtkLevelManager.Cartography;
using LDtkLevelManager.Utils;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager.Implementations.Basic
{
    public class WorldMap : MonoBehaviour
    {
        [SerializeField] private Project _project;

        [SerializeField]
        private MapLevelDrawer _mapLevelDrawerPrefab;

        [SerializeField]
        private GameObjectProvider _playerProvider;

        [SerializeField]
        private GameObject _characterPinPrefab;

        private Player _player;
        private Transform _characterPinTransform;

        private Cartographer _cartographer;
        private WorldInfo _currentWorld;

        private LdtkJson _projectJson;
        private float _scaledOffsetY;

        #region Behaviour

        private void Awake()
        {
            _cartographer = Cartographer.ForProject(_project);
            if (!ProjectService.Instance.TryGetLdtkJson(_project, out _projectJson))
            {
                Logger.Error($"Failed to load LDtkJson for project {_project.name}.", this);
                return;
            }

            _characterPinTransform = Instantiate(_characterPinPrefab, transform).transform;
            _scaledOffsetY = 0.75f * _cartographer.ScaleFactor;

            World world = _projectJson.Worlds[0];
            SetWorld(world);
        }

        private void OnEnable()
        {
            EvaluatePlayer();

            _playerProvider.Registered.AddListener(OnPlayerRegistered);
        }

        private void OnDisable()
        {
            _playerProvider.Registered.RemoveListener(OnPlayerRegistered);
        }

        private void Update()
        {
            if (_player == null) return;
            Vector2 newPos = _cartographer.CalculateScaledPosition(_player.transform.position);
            _characterPinTransform.position = new Vector3(
                newPos.x,
                newPos.y + _scaledOffsetY,
                transform.position.z - 1
            );
        }

        #endregion

        #region Player

        private void EvaluatePlayer()
        {
            if (_playerProvider.IsRegistered) return;
            _playerProvider.TryGetComponent(out _player);
        }

        private void OnPlayerRegistered(GameObject playerGO)
        {
            _player = playerGO.GetComponent<Player>();
        }

        #endregion

        public void SetWorld(World world)
        {
            _cartographer.TryWorldCartography(world, out WorldCartography worldCartography);

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
    }
}