using System.Threading.Tasks;
using LDtkUnity;
using LDtkVania;
using UnityEngine;

public class Starter : MonoBehaviour
{
    #region Inspector

    [SerializeField] private MV_LevelManager _levelManager;
    [SerializeField] private MV_Level _level;
    [SerializeField] private MV_PlayerControlBridge _playerControlBridge;

    #endregion

    #region Behaviour

    private void Start()
    {
        _ = LoadLevel();
    }

    #endregion

    #region Loading

    private async Task LoadLevel()
    {
        await _levelManager.FullLevelLoad(_level.Iid, MV_LevelLoadMode.LoadAndEnter);
        _playerControlBridge.GiveControl();
    }

    #endregion
}
