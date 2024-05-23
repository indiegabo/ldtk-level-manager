using System.Threading.Tasks;
using LDtkUnity;
using LDtkVania;
using UnityEngine;

public class Starter : MonoBehaviour
{
    #region Inspector

    [SerializeField] private MV_LevelManager _levelManager;
    [SerializeField] private MV_Level _level;
    [SerializeField] private Strategy _strategy;
    [SerializeField] private string _worldName;
    [SerializeField] private string _areaName;
    [SerializeField] private Transform _spawnPoint;

    #endregion

    #region Behaviour

    private void Start()
    {
        switch (_strategy)
        {
            case Strategy.Single:
                _ = LoadStandAloneLevel();
                break;
            case Strategy.World:
                _ = LoadWorld(_worldName, _level);
                break;
            case Strategy.Area:
                _ = LoadWorld(_worldName, _level);
                break;
        }
    }

    #endregion

    #region Loading

    private async Task LoadStandAloneLevel()
    {
        await _levelManager.LoadLevel(_level.Iid);
        _levelManager.Prepare(_level.Iid);
        _levelManager.Enter();
    }

    private async Task LoadWorld(string worldName, MV_Level level)
    {
        await _levelManager.LoadWorld(worldName);
        _levelManager.Prepare(level.Iid, _spawnPoint.position, 1);
        _levelManager.Enter();
    }

    private async Task LoadArea(string areaName, MV_Level level)
    {
        await Task.CompletedTask;
        // await _levelManager.LoadWorld(worldName);
        // _levelManager.PrepareLevel(level.Iid, _spawnPoint.position, 1);
        // _levelManager.EnterLevel();
    }

    #endregion

    private enum Strategy
    {
        Single,
        World,
        Area,
    }
}
