using LDtkLevelManager.EventBus;

namespace LDtkLevelManager
{
    /// <summary>
    /// Event that is raised when a level is activated.
    /// </summary>
    public class LevelActivationEvent : ILDtkLevelManagerEvent
    {
        /// <summary>
        /// The level behaviour that was activated .
        /// </summary>
        public LevelBehaviour behaviour;
    }

    /// <summary>
    /// Event that is raised when a level is deactivated.
    /// </summary>
    public class LevelDeactivationEvent : ILDtkLevelManagerEvent
    {
        /// <summary>
        /// The level behaviour that was deactivated.
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
        public FlowSubjectTrail trail;
    }
}
