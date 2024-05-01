using System.Collections.Generic;
using LDtkUnity;
using UnityEngine;

namespace LDtkVania
{
    [System.Serializable]
    public class MV_Area
    {
        [SerializeField]
        private string _iid;

        [SerializeField]
        public string _displayName;

        public string Iid => _iid;
        public string DisplayName { get => _displayName; set => _displayName = value; }

        public MV_Area(string iid)
        {
            _iid = iid;
        }
    }
}