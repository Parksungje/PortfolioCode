using System;
using Code.Entities;
using UnityEngine;

namespace Work.PSJ.Code.Boss.Skills
{
    public class BossSkillManagerCompo : MonoBehaviour, IEntityModule
    {
        private BossSkillCompo[] _skills = Array.Empty<BossSkillCompo>();

        public void Initialize(Entity entity)
        {
            _skills = entity.GetComponentsInChildren<BossSkillCompo>(true);
        }

        public T GetSkill<T>() where T : BossSkillCompo
        {
            for (int i = 0; i < _skills.Length; i++)
            {
                if (_skills[i] is T skill)
                    return skill;
            }

            return null;
        }

        public void SetAllSkillsEnabled(bool enabled)
        {
            for (int i = 0; i < _skills.Length; i++)
                _skills[i]?.SetSkillEnabled(enabled);
        }

        public void StopAllSkills()
        {
            for (int i = 0; i < _skills.Length; i++)
                _skills[i]?.StopSkill();
        }

        private void OnDisable()
        {
            StopAllSkills();
        }
    }
}
