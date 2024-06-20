
using System.Collections.Generic;
using UnityEngine;

namespace LDtkVania.Utils
{
    [System.Serializable]
    public class SerializableHashSet<TValue> : HashSet<TValue>, ISerializationCallbackReceiver
    {

        [HideInInspector]
        [SerializeField]
        private List<TValue> _values = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _values.Clear();

            foreach (var value in this)
            {
                _values.Add(value);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.Clear();

            for (int i = 0; i < _values.Count && i < _values.Count; i++)
            {
                Add(_values[i]);
            }
        }
    }
}