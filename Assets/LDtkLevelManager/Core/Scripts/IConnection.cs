namespace LDtkLevelManager
{
    /// <summary>
    /// Represents a connection between two levels in the Universe.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// The unique identifier of the connection.
        /// </summary>
        string Iid { get; }

        /// <summary>
        /// The unique identifier of the target level that the player can travel to.
        /// </summary>
        string TargetIid { get; }

        /// <summary>
        /// The placement spot where the player should spawn in the target level.
        /// </summary>
        IPlacementSpot Spot { get; }

        /// <summary>
        /// Initializes the connection. This should be called once the connection has been created.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Sets the active state of the connection. If set to false, the connection will not be used.
        /// </summary>
        /// <param name="isActive">The active state of the connection.</param>
        void SetActive(bool isActive);
    }

}