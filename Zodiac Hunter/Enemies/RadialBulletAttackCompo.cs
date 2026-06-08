using Code.Combat.Weapons;
using Code.Entities;
using UnityEngine;

namespace Work.PSJ.Code.Enemies
{
    public class RadialBulletAttackCompo : BasePatternAttackCompo
    {
        [SerializeField] private WeaponDataSO[] weaponItems;

        protected override WeaponDataSO[] GetActiveWeapons() => weaponItems;
    }
}
