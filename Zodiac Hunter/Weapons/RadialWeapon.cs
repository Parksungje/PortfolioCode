using Code.Combat;
using Code.Combat.Projectiles;
using Code.Combat.Weapons;
using Code.Entities;
using UnityEngine;

namespace Work.PSJ.Code.Weapons
{
    public class RadialWeapon : Weapon
    {
        protected override void ProjectileInitAndFire(Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            RadialWeaponDataSO radialData = weaponData as RadialWeaponDataSO;
            if (radialData == null || radialData.bulletCount <= 0)
                return;

            switch (radialData.patternType)
            {
                case RadialWeaponDataSO.PatternType.Circle:
                    FireCircle(radialData, direction, targetLayer, entity);
                    break;

                case RadialWeaponDataSO.PatternType.Fan:
                    FireFan(radialData, direction, targetLayer, entity);
                    break;

                case RadialWeaponDataSO.PatternType.Spiral:
                    FireSpiral(radialData, direction, targetLayer, entity);
                    break;

                case RadialWeaponDataSO.PatternType.DoubleSpiral:
                    FireDoubleSpiral(radialData, direction, targetLayer, entity);
                    break;

                case RadialWeaponDataSO.PatternType.TripleSpiral:
                    FireTripleSpiral(radialData, direction, targetLayer, entity);
                    break;

                case RadialWeaponDataSO.PatternType.RandomBurst:
                    FireRandomBurst(radialData, direction, targetLayer, entity);
                    break;
            }
        }

        private void FireCircle(RadialWeaponDataSO radialData, Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            float angleStep = 360f / radialData.bulletCount;

            for (int i = 0; i < radialData.bulletCount; i++)
            {
                float angle = radialData.startAngleOffset + angleStep * i;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * direction;
                SpawnBullet(dir, targetLayer, entity);
            }
        }

        private void FireFan(RadialWeaponDataSO radialData, Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            if (radialData.bulletCount == 1)
            {
                SpawnBullet(direction, targetLayer, entity);
                return;
            }

            float startAngle = -radialData.fanAngle * 0.5f + radialData.startAngleOffset;
            float angleStep = radialData.fanAngle / (radialData.bulletCount - 1);

            for (int i = 0; i < radialData.bulletCount; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * direction;
                SpawnBullet(dir, targetLayer, entity);
            }
        }

        private void FireSpiral(RadialWeaponDataSO radialData, Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            float angleStep = 360f / radialData.bulletCount;

            for (int i = 0; i < radialData.bulletCount; i++)
            {
                float angle = radialData.runtimeAngle + radialData.startAngleOffset + angleStep * i;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * direction;
                SpawnBullet(dir, targetLayer, entity);
            }

            radialData.runtimeAngle += radialData.spiralStepAngle;
        }

        private void FireDoubleSpiral(RadialWeaponDataSO radialData, Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            float angleStep = 360f / radialData.bulletCount;
            for (int i = 0; i < radialData.bulletCount; i++)
            {
                float cwAngle = radialData.runtimeAngle + radialData.startAngleOffset + angleStep * i;
                float ccwAngle = -radialData.runtimeAngle + radialData.startAngleOffset + 180f + angleStep * i;
                SpawnBullet(Quaternion.Euler(0f, 0f, cwAngle) * direction, targetLayer, entity);
                SpawnBullet(Quaternion.Euler(0f, 0f, ccwAngle) * direction, targetLayer, entity);
            }
            radialData.runtimeAngle += radialData.spiralStepAngle;
        }

        private void FireTripleSpiral(RadialWeaponDataSO radialData, Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            float angleStep = 360f / radialData.bulletCount;
            for (int arm = 0; arm < 3; arm++)
            {
                float armOffset = 120f * arm;
                for (int i = 0; i < radialData.bulletCount; i++)
                {
                    float angle = radialData.runtimeAngle + radialData.startAngleOffset + armOffset + angleStep * i;
                    SpawnBullet(Quaternion.Euler(0f, 0f, angle) * direction, targetLayer, entity);
                }
            }
            radialData.runtimeAngle += radialData.spiralStepAngle;
        }

        private void FireRandomBurst(RadialWeaponDataSO radialData, Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            for (int i = 0; i < radialData.bulletCount; i++)
            {
                float angle = Random.Range(0f, 360f);
                SpawnBullet(Quaternion.Euler(0f, 0f, angle) * direction, targetLayer, entity);
            }
        }

        private void SpawnBullet(Vector2 direction, LayerMask targetLayer, Entity entity)
        {
            Projectile bullet = _weaponHolder.poolManager.Pop(bulletPoolItem) as Projectile;
            if (!bullet)
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
        }
    }
}
