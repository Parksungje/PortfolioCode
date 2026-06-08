using Code.Combat.Weapons;
using UnityEngine;

namespace Work.PSJ.Code.Weapons
{
    [CreateAssetMenu(fileName = "HomingWeaponData", menuName = "SO/SJ/HomingWeaponDataSO")]
    public class HomingWeaponDataSO : WeaponDataSO
    {
        public float homingStrength = 4f;

        public float homingDelay = 0.2f;
    }
}