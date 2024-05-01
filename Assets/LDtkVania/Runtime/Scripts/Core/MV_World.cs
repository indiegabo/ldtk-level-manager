using System.Collections.Generic;
using LDtkUnity;
using UnityEngine;

namespace LDtkVania
{
    [System.Serializable]
    public class MV_World
    {
        [SerializeField]
        private string _iid;

        [SerializeField]
        public string _ldtkName;

        [SerializeField]
        public string _displayName;

        [SerializeField]
        public List<MV_Area> _areas = new();

        public string Iid => _iid;
        public string Name => !string.IsNullOrEmpty(_displayName) ? _displayName : _ldtkName;
        public string LDtkName => _ldtkName;
        public string DisplayName { get => _displayName; set => _displayName = value; }

        public MV_World(World world)
        {
            _iid = world.Iid;
            _ldtkName = world.Identifier;
        }

        public void UpdateInfo(World world)
        {
            _ldtkName = world.Identifier;
        }
    }
}