using LDtkVania;
using LDtkVania.Utils;
using TarodevController;
using UnityEngine;

public class Character : MonoBehaviour, ILevelPlacementSubject
{
    [SerializeField] private GameObjectProvider _characterProvider;
    [SerializeField] private MV_PlayerControlBridge _playerControlBridge;

    private PlayerController _playerController;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _characterProvider.Register(gameObject);
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        _playerControlBridge.PlayerControlChanged.AddListener(OnPlayerControlChanged);
        if (_playerControlBridge.IsControlled)
        {
            _playerController.GiveControl();
        }
    }

    private void OnDisable()
    {
        _playerControlBridge.PlayerControlChanged.RemoveListener(OnPlayerControlChanged);
    }

    private void OnDestroy()
    {
        _characterProvider.Unregister();
    }

    public void PlaceInLevel(Vector2 position, int directionSign)
    {
        transform.position = new Vector3(position.x, position.y - 0.5f, transform.position.z);
    }

    private void OnPlayerControlChanged(bool shouldControl)
    {
        if (shouldControl)
        {
            _rb.velocity = Vector3.zero;
            _playerController.GiveControl();
        }
        else
        {
            _rb.velocity = Vector3.zero;
            _playerController.RemoveControl();
        }
    }
}
