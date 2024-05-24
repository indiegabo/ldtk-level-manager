using UnityEngine;
using LDtkVania.Cartography;

namespace Tests
{
    public class WorldDrawer : MonoBehaviour
    {
        #region Inspector

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

        }

        #endregion
    }
}