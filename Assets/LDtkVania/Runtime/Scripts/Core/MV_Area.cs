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
        public string _name;

        public string Iid => _iid;
        public string Name => _name;

        public MV_Area(string iid, string name)
        {
            _iid = iid;
            _name = name;
        }
    }
}