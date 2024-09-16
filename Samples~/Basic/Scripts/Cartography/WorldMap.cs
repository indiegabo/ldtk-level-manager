using System;
using System.Collections.Generic;
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
        [SerializeField] private GameObject _characterPinPrefab;
        [SerializeField] private Transform _levelsContainer;
        [SerializeField] private Camera _camera;

        private Transform _characterPinTransform;

        private Cartographer _cartographer;
        private World _currentWorld;

        private LdtkJson _projectJson;
        private float _scaledOffsetY;

        private Dictionary<string, World> _worlds = new();

        #region Behaviour

        private void Awake()
        {
            _cartographer = Cartographer.For(_project);

            if (!ProjectsService.Instance.TryGetLdtkJson(_project, out _projectJson))
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
            LevelLoader.Instance.LevelPrepared.AddListener(OnLevelPrepared);
        }

        private void OnDisable()
        {
            LevelLoader.Instance.LevelPrepared.RemoveListener(OnLevelPrepared);
        }

        private void Update()
        {
            if (Player.Instance == null) return;

            Vector2 newPos = _cartographer.CalculateScaledPosition(Player.Instance.transform.position);
            _characterPinTransform.position = new Vector3(
                newPos.x,
                newPos.y + _scaledOffsetY,
                transform.position.z - 1
            );

            _camera.transform.position = new Vector3(
                newPos.x,
                newPos.y,
                transform.position.z - 10
            );
        }

        #endregion

        #region Navigation        

        private void OnLevelPrepared(LevelBehaviour levelBehaviour, ILevelFlowSubject subject, LevelTrail trail)
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