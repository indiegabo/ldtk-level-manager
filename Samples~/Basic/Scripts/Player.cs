using LDtkLevelManager;
using LDtkLevelManager.Utils;
using TarodevController;
using UnityEngine;

namespace LDtkLevelManager.Implementations.Basic
{
    public class Player : MonoBehaviour, ICharacterLevelFlowSubject
    {
        [SerializeField] private GameObjectProvider _characterProvider;

        private PlayerController _playerController;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _characterProvider.Register(gameObject);
            _playerController = GetComponent<PlayerController>();
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
}
