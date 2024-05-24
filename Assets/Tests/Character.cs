using LDtkVania;
using LDtkVania.Cartography;
using LDtkVania.Utils;
using Sirenix.OdinInspector;
using TarodevController;
using UnityEngine;

public class Character : MonoBehaviour, ICharacterLevelFlowSubject
{
    [SerializeField] private GameObjectProvider _characterProvider;

    private PlayerController _playerController;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _characterProvider.Register(gameObject);
        _playerController = GetComponent<PlayerController>();
        _playerController.RemoveControl();
    }

    public void OnLevelExit()
    {
        _playerController.RemoveControl();
    }

    public void PlaceInLevel(Vector2 position, int directionSign)
    {
        transform.position = new Vector3(position.x, position.y - 0.5f, transform.position.z);
    }

    public void OnLevelEnter()
    {
        _playerController.GiveControl();
    }
}
