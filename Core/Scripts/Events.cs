using LDtkLevelManager.EventBus;

namespace LDtkLevelManager
{
    /// <summary>
    /// Event that is raised when a level is activated or deactivated.
    /// </summary>
    public class LevelActivationEvent : ILDtkLevelManagerEvent
    {
        /// <summary>
        /// Whether the level was activated or deactivated.
        /// </summary>
        public bool isActive;
        /// <summary>
        /// The level behaviour that was activated or deactivated.
        /// </summary>
        public LevelBehaviour behaviour;
    }

    /// <summary>
    /// Event that is raised when a level is prepared.
    /// </summary>
    public class LevelPreparationEvent : ILDtkLevelManagerEvent
    {
        /// <summary>
        /// The level behaviour that was prepared.
        /// </summary>
        public LevelBehaviour behaviour;
        /// <summary>
        /// The level flow subject from which the level was prepared.
        /// </summary>
        public ILevelFlowSubject subject;
        /// <summary>
        /// The trail that was used to prepare the level.
        /// </summary>
        public LevelTrail trail;
    }
}
