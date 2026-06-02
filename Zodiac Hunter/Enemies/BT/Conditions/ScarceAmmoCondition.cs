using System;
using Code.Combat;
using Code.Combat.Weapons;
using Unity.Behavior;
using UnityEngine;

namespace Work.PSJ.Code.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "ScarceAmmoCondition", story: "Scarce [Weapon] Ammo", category: "Conditions", id: "916a3c700c4b50ad8d565a835f3f35cd")]
    public partial class ScarceAmmoCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<Weapon> Weapon;

        public override bool IsTrue()
        {
            if (Weapon == null || Weapon.Value == null)
                return false;

            return Weapon.Value.weaponData != null && Weapon.Value.weaponData.currentAmmo <= 0;
        }
    }
}
