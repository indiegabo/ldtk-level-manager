using UnityEngine;
using LDtkVania.Cartography;
using System.Collections.Generic;

namespace Tests
{
    public class WorldDrawer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private MapLevelDrawer _mapLevelDrawerPrefab;

        [SerializeField]
        private Transform _characterTransform;

        [SerializeField]
        private GameObject _characterPinPrefab;

        #endregion

        #region Fields

        private MV_Cartographer _cartographer;
        private SpriteRenderer _renderer;
        private Transform _characterPinTransform;
        private float _scaledOffsetY;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _cartographer = GetComponent<MV_Cartographer>();
            _renderer = GetComponent<SpriteRenderer>();

            _characterPinTransform = Instantiate(_characterPinPrefab).transform;

            _scaledOffsetY = 0.75f * _cartographer.ScaleFactor;
        }

        private void Start()
        {
            if (!_cartographer.TryGetWorld("City", out MV_WorldCartography worldCartography)) return;

            Vector2 worldCenter = worldCartography.Bounds.ScaledCenter;
            transform.position = new Vector3(worldCenter.x, worldCenter.y, transform.position.z);

            foreach (MV_AreaCartography area in worldCartography.GetAllAreas())
            {
                foreach (MV_LevelCartography levelCartography in area.GetAllLevels())
                {
                    MapLevelDrawer mapLevelDrawer = Instantiate(_mapLevelDrawerPrefab, transform);
                    mapLevelDrawer.Initialize(levelCartography, transform.position.z);
                }
            }

            _characterPinTransform.localScale *= _cartographer.ScaleFactor;
        }

        private void Update()
        {
            Vector2 newPos = _cartographer.CalculateScaledPosition(_characterTransform.position);
            _characterPinTransform.position = new Vector3(newPos.x, newPos.y + _scaledOffsetY, _characterPinTransform.position.z);
        }

        #endregion
    }
}