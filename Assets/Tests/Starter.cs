using LDtkUnity;
using LDtkVania;
using UnityEngine;

public class Starter : MonoBehaviour
{
    #region Inspector

    [SerializeField] private MV_LevelManager _levelManager;
    [SerializeField] private LDtkLevelFile _levelFile;

    #endregion

    #region Behaviour

    private void Awake()
    {
        Level level = _levelFile.FromJson;
        _ = _levelManager.FullLevelLoad(level.Iid, MV_LevelLoadMode.LoadAndEnter);
    }

    #endregion
}
