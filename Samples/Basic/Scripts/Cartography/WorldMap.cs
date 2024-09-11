using System;
using System.Collections.Generic;
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
        [SerializeField] private MapLevelDrawer _mapLevelDrawerPrefab;
        [SerializeField] private GameObjectProvider _playerProvider;
        [SerializeField] private GameObject _characterPinPrefab;
        [SerializeField] private LevelNavigationBridge _navigationBridge;
        [SerializeField] private Transform _levelsContainer;

        private Player _player;
        private Transform _characterPinTransform;

        private Cartographer _cartographer;
        private World _currentWorld;

        private LdtkJson _projectJson;
        private float _scaledOffsetY;

        private Dictionary<string, World> _worlds = new();

        #region Behaviour

        private void Awake()
        {
            _cartographer = Cartographer.ForProject(_project);
            if (!ProjectService.Instance.TryGetLdtkJson(_project, out _projectJson))
            {
                Logger.Error($"Failed to load LDtkJson for project {_project.name}.", this);
                return;
            }

            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                _project.LanesSettings.MapRenderingLane.StartingZ
            );
            _levelsContainer.position = transform.position;
            _characterPinTransform = Instantiate(_characterPinPrefab, transform).transform;
            _characterPinTransform.localScale *= _cartographer.ScaleFactor;
            _scaledOffsetY = 0.75f * _cartographer.ScaleFactor;

            foreach (World world in _projectJson.Worlds)
            {
                _worlds[world.Iid] = world;
            }
        }

        private void OnEnable()
        {
            EvaluatePlayer();
            _playerProvider.Registered.AddListener(OnPlayerRegistered);
            _navigationBridge.LevelPrepared.AddListener(OnLevelPrepared);
        }

        private void OnDisable()
        {
            _playerProvider.Registered.RemoveListener(OnPlayerRegistered);
            _navigationBridge.LevelPrepared.RemoveListener(OnLevelPrepared);
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

        #region Navigation        

        private void OnLevelPrepared(LevelBehaviour levelBehaviour, LevelTrail trail)
        {
            if (!_worlds.TryGetValue(levelBehaviour.Info.WorldIid, out World world)) return;
            if (_currentWorld != world)
            {
                SetWorld(world);
            }
        }

        public void SetWorld(World world)
        {
            foreach (Transform child in _levelsContainer)
            {
                Destroy(child.gameObject);
            }

            _cartographer.TryWorldCartography(world, out WorldCartography worldCartography);

            Vector2 worldCenter = worldCartography.Bounds.ScaledCenter;
            transform.position = new Vector3(worldCenter.x, worldCenter.y, transform.position.z);

            foreach (AreaCartography area in worldCartography.GetAllAreas())
            {
                foreach (LevelCartography levelCartography in area.GetAllLevels())
                {
                    MapLevelDrawer mapLevelDrawer = Instantiate(_mapLevelDrawerPrefab, _levelsContainer);
                    mapLevelDrawer.Initialize(levelCartography, transform.position.z);
                }
            }
            _currentWorld = world;
        }

        #endregion
    }
}