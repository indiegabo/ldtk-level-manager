using UnityEngine;
using UnityEngine.Events;

namespace LDtkVania.Utils
{
    [CreateAssetMenu(fileName = "GameObjectProvider", menuName = "LDtkVania/Providers/GameObjectProvider")]
    public class GameObjectProvider : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        private UnityEvent<GameObject> _registered;

        [SerializeField]
        private UnityEvent _unregistered;

        #endregion

        #region Fields

        private GameObject _subject;

        #endregion

        #region Getters

        public bool IsRegistered => _subject != null;
        public UnityEvent<GameObject> Registered => _registered;
        public UnityEvent Unregistered => _unregistered;

        #endregion

        #region Registering

        public void Register(GameObject gameObject)
        {
            _subject = gameObject;
            _registered?.Invoke(_subject);
        }

        public void Unregister()
        {
            _subject = null;
            _unregistered?.Invoke();
        }

        #endregion

        #region Providing

        public GameObject Get()
        {
            return _subject;
        }

        public bool TryGet(out GameObject gameObject)
        {
            gameObject = _subject;
            return _subject != null;
        }

        public T GetComponent<T>()
        {
            return _subject.GetComponent<T>();
        }

        public T GetComponentInChildren<T>()
        {
            return _subject.GetComponentInChildren<T>();
        }

        public T GetComponentInParent<T>()
        {
            return _subject.GetComponentInParent<T>();
        }

        public bool TryGetComponent<T>(out T component)
        {
            if (_subject == null) { component = default; return false; }
            component = _subject.GetComponent<T>();
            return component != null;
        }


        #endregion
    }
}