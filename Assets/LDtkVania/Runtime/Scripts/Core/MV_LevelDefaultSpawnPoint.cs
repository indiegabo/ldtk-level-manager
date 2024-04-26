using UnityEngine;
using LDtkUnity;
namespace LDtkVania
{
    public class MV_LevelDefaultSpawnPoint : MonoBehaviour
    {
        #region Fields

        private LDtkFields _fields;
        private int _directionSign;

        #endregion

        #region Getters

        public int DirectionSign => _directionSign;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _fields = GetComponent<LDtkFields>();
            _directionSign = _fields.GetInt("DirectionSign");
        }

        #endregion
    }
}