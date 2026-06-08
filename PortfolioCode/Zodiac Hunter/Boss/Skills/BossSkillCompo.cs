using Code.Combat;
using Code.Entities;
using UnityEngine;

namespace Work.PSJ.Code.Boss.Skills
{
    public abstract class BossSkillCompo : MonoBehaviour, IEntityModule
    {
        [SerializeField] private bool skillEnabled = true;

        public bool IsSkillEnabled => skillEnabled;
        protected Entity OwnerEntity { get; private set; }
        protected AbstractBoss OwnerBoss { get; private set; }
        protected CombatCalculator CombatCalculator { get; private set; }

        protected bool CanUseSkill => skillEnabled && isActiveAndEnabled;

        public void Initialize(Entity entity)
        {
            OwnerEntity = entity;
            OwnerBoss = entity as AbstractBoss;
            CombatCalculator = entity.GetModule<CombatCalculator>();
            OnInitializeSkill(entity);
        }

        public void SetSkillEnabled(bool enabled)
        {
            if (skillEnabled == enabled)
                return;

            skillEnabled = enabled;
            if (!skillEnabled)
                StopSkill();

            OnSkillEnabledChanged(skillEnabled);
        }

        public virtual void StopSkill()
        {
        }

        protected virtual void OnInitializeSkill(Entity entity)
        {
        }

        protected virtual void OnSkillEnabledChanged(bool enabled)
        {
        }
    }
}
