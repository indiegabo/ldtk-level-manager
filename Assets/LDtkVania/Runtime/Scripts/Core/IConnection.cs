using UnityEngine;

namespace LDtkVania
{
    public interface IConnection
    {
        string Iid { get; }
        string TargetIid { get; }
        ILevelAnchor Anchor { get; }

        void Initialize();
        void SetActive(bool isActive);
    }
}