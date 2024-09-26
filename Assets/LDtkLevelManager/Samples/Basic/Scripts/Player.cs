using LDtkLevelManager.Utils;
using TarodevController;
using UnityEngine;

namespace LDtkLevelManager.Implementations.Basic
{
    public class Player : MonoBehaviour, ILevelFlowSubject
    {
        private static Player _currentInstance;
        public static Player Instance => _currentInstance;

        [SerializeField] private SpriteRenderer _spriteRenderer;

        private PlayerController _playerController;

        private void Awake()
        {
            if (_currentInstance != null && _currentInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            _currentInstance = this;
            _playerController = GetComponent<PlayerController>();
        }

        public void LeaveLevel(ConnectedLevelBehaviour levelBehaviour)
        {
            _playerController.RemoveControl();
        }

        public void PlaceInLevel(ConnectedLevelBehaviour levelBehaviour, Vector3 position, int directionSign)
        {
            _spriteRenderer.flipX = directionSign < 0;
            transform.position = new Vector3(position.x, position.y - 0.5f, position.z);
        }

        public void EnterLevel(ConnectedLevelBehaviour levelBehaviour)
        {
            _playerController.GiveControl();
        }
    }
}
