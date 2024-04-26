using UnityEngine;
using LDtkUnity;

namespace LDtkVania
{
    [DefaultExecutionOrder(-1000000)]
    public class MV_LevelInstantiator : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private GameObject _ldtkLevelFile;

        private void InstantiateLevel()
        {
            _levelGameObject = Instantiate(_ldtkLevelFile);
        }

        [SerializeField]
        private GameObject _levelGameObject;

        #endregion

        #region Behaviour

        private void Awake()
        {
            if (_levelGameObject != null)
            {
                Destroy(_levelGameObject);
            }

            Instantiate(_ldtkLevelFile);
            Destroy(gameObject);
        }

        #endregion
    }
}