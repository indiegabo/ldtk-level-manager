using LDtkLevelManager;
using LDtkLevelManager.Utils;
using TarodevController;
using UnityEngine;

namespace LDtkLevelManager.Implementations.Basic
{
    public class Player : MonoBehaviour, ICharacterLevelFlowSubject
    {
        [SerializeField] private GameObjectProvider _characterProvider;
        [SerializeField] private SpriteRenderer _spriteRenderer;

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

        public void PlaceInLevel(Vector3 position, int directionSign)
        {
            transform.position = new Vector3(position.x, position.y - 0.5f, position.z);
            _spriteRenderer.flipX = directionSign < 0;
        }

        public void OnLevelEnter()
        {
            _playerController.GiveControl();
        }
    }
}
