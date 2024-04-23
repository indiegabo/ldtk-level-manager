using UnityEngine;
using LDtkUnity;
namespace LDtkVania
{
    public class MV_LevelDefaultSpawnPoint : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private LDtkFields _fields;

        [SerializeField]
        private string _directionSignKey;

        [SerializeField]
        private int _directionSign;

        #endregion

        #region Fields

        #endregion

        #region Getters

        public int DirectionSign => _directionSign;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _fields = GetComponent<LDtkFields>();
            _directionSign = _fields.GetInt(_directionSignKey);
        }

        #endregion

        #region LDtk Fields

        public void OnLDtkImportFields(LDtkFields fields)
        {
            _fields = fields;
            _directionSign = fields.GetInt(_directionSignKey);
        }

        #endregion
    }
}