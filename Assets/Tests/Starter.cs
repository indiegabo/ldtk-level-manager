using System.Threading.Tasks;
using LDtkUnity;
using LDtkVania;
using UnityEngine;

public class Starter : MonoBehaviour
{
    #region Inspector

    [SerializeField] private MV_LevelManager _levelManager;
    [SerializeField] private MV_Level _level;
    [SerializeField] private string _worldName;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private MV_PlayerControlBridge _playerControlBridge;

    #endregion

    #region Behaviour

    private void Start()
    {
        if (!string.IsNullOrEmpty(_worldName))
        {
            _ = LoadWorld(_worldName, _level);
            return;
        }
        _ = LoadStandAloneLevel();
    }

    #endregion

    #region Loading

    private async Task LoadStandAloneLevel()
    {
        await _levelManager.LoadLevelAndNeighbours(_level.Iid, MV_LevelLoadMode.LoadOnly);
        _levelManager.PrepareLevel(_level.Iid);
        _levelManager.EnterLevel();
        _playerControlBridge.GiveControl();
    }

    private async Task LoadWorld(string worldName, MV_Level level)
    {
        await _levelManager.LoadWorld(worldName);
        _levelManager.PrepareLevel(level.Iid, _spawnPoint.position, 1);
        _levelManager.EnterLevel();
        _playerControlBridge.GiveControl();
    }

    #endregion
}
