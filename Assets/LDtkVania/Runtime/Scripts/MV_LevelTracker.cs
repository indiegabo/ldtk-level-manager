using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania
{
    [CreateAssetMenu(fileName = "Level Tracker", menuName = "Handy Tools/Metroidvania/Gameplay/Levels/Level Tracker")]
    public class MV_LevelTracker : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        private UnityEvent<MV_Level> _enteredLevel;

        #endregion

        #region Fields

        private MV_Level _currentLevel;

        #endregion

        #region Getters

        public bool HasCurrentLevel => _currentLevel != null;
        public MV_Level CurrentLevel => _currentLevel;

        public UnityEvent<MV_Level> EnteredLevel => _enteredLevel;

        #endregion

        #region Setting

        public void DefineCurrentLevel(string iid)
        {
            if (!MV_Project.Instance.TryGetLevel(iid, out _currentLevel))
            {
                MV_Logger.Error($"{name} could not define {iid} as current level because it is not present on project's dictionary", this);
                return;
            }
            _enteredLevel.Invoke(_currentLevel);
        }

        public void RemoveAsCurrentLevel(string iid)
        {
            if (!MV_Project.Instance.TryGetLevel(iid, out MV_Level mvLevel))
            {
                MV_Logger.Error($"{name} could not remove {iid} as current level because it is not present on project's dictionary", this);
                return;
            }

            if (mvLevel != _currentLevel) return;
            _currentLevel = null;
        }

        #endregion
    }
}
