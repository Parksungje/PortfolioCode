using Code.Combat;
using Code.Combat.Weapons;
using Code.Entities;
using Code.Player;
using UnityEngine;

namespace Work.PSJ.Code.Weapons
{
    public class HomingWeapon : Weapon
    {
        [SerializeField] private PlayerSO playerSO;

        protected override void ProjectileInitAndFire(Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            HomingWeaponDataSO homingData = weaponData as HomingWeaponDataSO;
            if (homingData == null)
                return;

            HomingProjectile bullet = _weaponHolder.poolManager.Pop(bulletPoolItem) as HomingProjectile;
            if (bullet == null)
                return;

            DamageData damageData = _combatCalculator.CalculateDamage(weaponData.damage);

            bullet.FireAndInit(
                Muzzle.position,
                direction.normalized,
                weaponData.bulletSpeed,
                damageData,
                weaponData.knockbackForce,
                weaponData.knockbackDuration,
                targetLayer,
                entity
            );

            if (playerSO != null && playerSO.player != null)
                bullet.SetupHoming(playerSO.player.transform, homingData.homingStrength, homingData.homingDelay);
        }
    }
}