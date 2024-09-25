using LDtkLevelManager.EventBus;

namespace LDtkLevelManager
{
    public class LevelActivationEvent : ILDtkLevelManagerEvent
    {
        public bool isActive;
        public LevelBehaviour behaviour;
    }

    public class LevelPreparationEvent : ILDtkLevelManagerEvent
    {
        public LevelBehaviour behaviour;
        public ILevelFlowSubject subject;
        public LevelTrail trail;
    }
}
