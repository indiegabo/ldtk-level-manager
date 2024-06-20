namespace LDtkLevelManager
{
    public interface IConnection
    {
        string Iid { get; }
        string TargetIid { get; }
        IPlacementSpot Spot { get; }

        void Initialize();
        void SetActive(bool isActive);
    }
}