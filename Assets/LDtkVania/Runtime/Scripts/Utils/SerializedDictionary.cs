using System.Collections.Generic;
using UnityEngine;

namespace LDtkVania
{
    [System.Serializable]
    public abstract class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        #region Inspector

        [HideInInspector]
        [SerializeField]
        private List<TKey> _keys = new();

        [HideInInspector]
        [SerializeField]
        private List<TValue> _values = new();

        #endregion

        #region Properties

        protected List<TKey> keys => _keys;
        protected List<TValue> values => _values;

        #endregion

        #region  Serialization Callbacks

        public void OnAfterDeserialize()
        {
            this.Clear();

            for (int i = 0; i < _keys.Count && i < _values.Count; i++)
            {
                this[_keys[i]] = _values[i];
            }
        }

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var item in this)
            {
                _keys.Add(item.Key);
                _values.Add(item.Value);
            }
        }

        #endregion
    }
}