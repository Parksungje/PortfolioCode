using GondrLib.Events;

namespace Work.PSJ.Code.Boss.Events
{
    public struct BossHealthChangedEvent : IEvent
    {
        public float MaxHealth;
        public float CurrentHealth;

        public BossHealthChangedEvent(float max, float current)
        {
            MaxHealth = max;
            CurrentHealth = current;
        }
    }

    public struct BossPhaseChangedEvent : IEvent
    {
        public AbstractBoss.BossState Phase;

        public BossPhaseChangedEvent(AbstractBoss.BossState phase)
        {
            Phase = phase;
        }
    }
}