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

        /// <summary>
        /// Gets the registered game object.
        /// </summary>
        /// <value>The registered game object, or null if not registered.</value>
        public GameObject Get()
        {
            return _subject;
        }

        /// <summary>
        /// Attempts to get the registered game object.
        /// </summary>
        /// <param name="gameObject">The registered game object, or the default value if not registered.</param>
        /// <returns><c>true</c> if the game object was found.</returns>
        public bool TryGet(out GameObject gameObject)
        {
            gameObject = _subject;
            return _subject != null;
        }

        /// <summary>
        /// Gets a component of the given type from the registered game object. <br />
        /// <br />
        /// </summary>
        /// <returns>The component or the default value if not found.</returns>
        public T GetComponent<T>()
        {
            return _subject.GetComponent<T>();
        }

        /// <summary>
        /// Gets a component of the given type from the children of the registered game object. <br />
        /// <br />
        /// </summary>
        /// <returns>The component or the default value if not found.</returns>
        public T GetComponentInChildren<T>()
        {
            return _subject.GetComponentInChildren<T>();
        }

        /// <summary>
        /// Gets a component of the given type from the parent of the registered game object. <br />
        /// <br />
        /// </summary>
        /// <returns>The component or the default value if not found.</returns>
        public T GetComponentInParent<T>()
        {
            return _subject.GetComponentInParent<T>();
        }

        /// <summary>
        /// Attempts to get a component of a given type from the registered game object. <br/>
        /// If the component is not found, the <paramref name="component" /> parameter will be set to the default value.<br/>
        /// </summary>
        /// <param name="component">The found component, or the default value if not found.</param>
        /// <returns><c>true</c> if the component was found.</returns>
        public bool TryGetComponent<T>(out T component)
        {
            if (_subject == null) { component = default; return false; }
            component = _subject.GetComponent<T>();
            return component != null;
        }

        #endregion
    }
}