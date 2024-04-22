using UnityEngine;
using UnityEngine.Events;

namespace HandyFSM
{
    [System.Serializable]
    public class TriggerListItem
    {
        [SerializeField]
        private string _key;

        [SerializeField]
        private UnityEvent _trigger;

        public string Key => _key;
        public UnityEvent Trigger => _trigger;
    }
}