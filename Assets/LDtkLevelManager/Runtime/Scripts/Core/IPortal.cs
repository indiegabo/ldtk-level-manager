namespace LDtkLevelManager
{
    public interface IPortal
    {
        string Iid { get; }
        string TargetIid { get; }
        IPlacementSpot Spot { get; }
        void SetActive(bool isActive);
    }
}