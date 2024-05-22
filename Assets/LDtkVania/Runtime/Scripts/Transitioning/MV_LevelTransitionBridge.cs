using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania.Transitioning
{
    [CreateAssetMenu(fileName = "Level Transition Bridge", menuName = "LDtkVania/Transitioning/Transition Bridge", order = 0)]
    public class MV_LevelTransitionBridge : ScriptableObject
    {
        #region Fields

        private MV_LevelTransitioner _levelTransitioner;

        #endregion

        #region Registering

        public void Register(MV_LevelTransitioner levelTransitioner)
        {
            _levelTransitioner = levelTransitioner;
        }

        public void ClearRegistry()
        {
            _levelTransitioner = null;
        }

        #endregion

        #region Transitioning

        #endregion
    }
}