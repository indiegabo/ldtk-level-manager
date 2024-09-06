namespace LDtkLevelManager
{
    /// <summary>
    /// Represents a portal in a level that the player can pass through to
    /// travel to another level.
    /// </summary>
    public interface IPortal
    {
        /// <summary>
        /// The LDtk unique identifier of the portal.
        /// </summary>
        string Iid { get; }

        /// <summary>
        /// The LDtk unique identifier of the target level.
        /// </summary>
        string TargetIid { get; }

        /// <summary>
        /// The <see cref="IPlacementSpot"/> of the player after passing through the portal.
        /// </summary>
        IPlacementSpot Spot { get; }

        /// <summary>
        /// Sets the active state of the portal.
        /// </summary>
        /// <param name="isActive">Whether the portal is active or not.</param>
        void SetActive(bool isActive);
    }
}
