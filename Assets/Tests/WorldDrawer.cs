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

        #endregion

        #region Fields

        private MV_Cartographer _cartographer;
        private SpriteRenderer _renderer;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _cartographer = GetComponent<MV_Cartographer>();
            _renderer = GetComponent<SpriteRenderer>();
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
        }

        #endregion
    }
}