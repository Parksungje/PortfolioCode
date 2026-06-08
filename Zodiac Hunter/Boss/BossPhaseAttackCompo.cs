using Code.Combat.Weapons;
using Code.Entities;
using UnityEngine;
using Work.PSJ.Code.Enemies;

namespace Work.PSJ.Code.Boss
{
    public class BossPhaseAttackCompo : BasePatternAttackCompo
    {
        [SerializeField] private WeaponDataSO[] phase1Weapons;
        [SerializeField] private WeaponDataSO[] phase2Weapons;
        [SerializeField] private WeaponDataSO[] phase3Weapons;

        [SerializeField] [Range(0.1f, 1f)] private float phase2IntervalMultiplier = 0.65f;
        [SerializeField] [Range(0.1f, 1f)] private float phase3IntervalMultiplier = 0.35f;

        private WeaponDataSO[] _currentWeapons;
        private AbstractBoss _boss;
        private float _intervalMultiplier = 1f;

        public override void Initialize(Entity entity)
        {
            base.Initialize(entity);
            _currentWeapons = phase1Weapons;

            if (entity is AbstractBoss boss)
            {
                _boss = boss;
                _boss.OnPhaseChanged.AddListener(HandlePhaseChanged);
            }
        }

        protected override WeaponDataSO[] GetActiveWeapons() => _currentWeapons;
        protected override float GetCurrentInterval() => patternChangeInterval * _intervalMultiplier;

        private void OnDestroy()
        {
            if (_boss != null)
                _boss.OnPhaseChanged.RemoveListener(HandlePhaseChanged);
        }

        private void HandlePhaseChanged(AbstractBoss.BossState phase)
        {
            _currentWeapons = phase switch
            {
                AbstractBoss.BossState.PHASE2 => phase2Weapons,
                AbstractBoss.BossState.PHASE3 => phase3Weapons,
                _ => phase1Weapons
            };

            _intervalMultiplier = phase switch
            {
                AbstractBoss.BossState.PHASE2 => phase2IntervalMultiplier,
                AbstractBoss.BossState.PHASE3 => phase3IntervalMultiplier,
                _ => 1f
            };

            StopAttack();
            ResetWeaponIndex();
            EquipWeapon(0);
        }
    }
}
