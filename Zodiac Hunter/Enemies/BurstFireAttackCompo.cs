using System.Collections;
using Code.Combat;
using Code.Combat.Weapons;
using Code.Entities;
using UnityEngine;

namespace Work.PSJ.Code.Enemies
{
    public class BurstFireAttackCompo : MonoBehaviour, IEntityModule, IAfterInitialize, IEnemyAttackModule
    {
        [SerializeField] private WeaponDataSO weaponItem;
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstInterval = 0.12f;
        [SerializeField] private float burstCooldown = 1.5f;

        public bool isReloading => false;

        private WeaponHolder _weaponHolder;
        private float _lastBurstTime = -999f;
        private bool _isBursting;

        public void Initialize(Entity entity)
        {
            _weaponHolder = entity.GetModule<WeaponHolder>();
            if (_weaponHolder == null)
                Debug.LogError($"WeaponHolder not found on {entity.name}");
        }

        public void AfterInitialize()
        {
            if (weaponItem == null || _weaponHolder == null) return;

            WeaponDataSO clone = weaponItem.Clone() as WeaponDataSO;
            if (clone == null) return;

            clone.currentAmmo = int.MaxValue;
            clone.shotDelay = 0f;
            weaponItem = clone;
            _weaponHolder.SwapWeapon(weaponItem);
        }

        public void SetAim(Vector2 targetPoint)
        {
            _weaponHolder?.SetAim(targetPoint);
        }

        public void Attack(Transform target)
        {
            if (target == null || _weaponHolder == null || _isBursting)
                return;

            if (Time.time < _lastBurstTime + burstCooldown)
                return;

            _lastBurstTime = Time.time;
            StartCoroutine(BurstRoutine(target));
        }

        private IEnumerator BurstRoutine(Transform target)
        {
            _isBursting = true;

            for (int i = 0; i < burstCount; i++)
            {
                if (target == null || _weaponHolder == null)
                    break;

                _weaponHolder.SetAim(target.position);
                _weaponHolder.HandleAttack(true);
                yield return null;
                _weaponHolder.HandleAttack(false);

                if (i < burstCount - 1)
                    yield return new WaitForSeconds(burstInterval);
            }

            _isBursting = false;
        }

        public void StartAttack() { }

        public void StopAttack()
        {
            _weaponHolder?.HandleAttack(false);
        }

        public void Reload() { }
        public bool CanReload() => false;
        public bool NeedReload() => false;

        private void OnDisable()
        {
            StopAllCoroutines();
            _isBursting = false;
            _weaponHolder?.HandleAttack(false);
        }
    }
}