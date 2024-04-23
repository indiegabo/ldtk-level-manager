using UnityEngine;

namespace LDtkVania
{
    public class MV_ProjectInitializer : MonoBehaviour
    {
        #region Fields

        private MV_Project _project;

        #endregion

        #region Behaviour

        #endregion

        #region Initialization

        public void Initialize()
        {
            _project = MV_Project.Instance;
        }

        #endregion
    }
}