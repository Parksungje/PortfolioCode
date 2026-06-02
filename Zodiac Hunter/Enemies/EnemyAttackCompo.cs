using Code.Combat;
using Code.Combat.Weapons;
using Code.Entities;
using DG.Tweening;
using UnityEngine;

namespace Work.PSJ.Code.Enemies
{
    public class EnemyAttackCompo : MonoBehaviour, IEntityModule, IAfterInitialize, IEnemyAttackModule
    {
        [SerializeField] private WeaponDataSO weaponItem;
        [SerializeField] private bool randomizeShotDelay = false;
        [SerializeField] private float minShotDelay = 0.5f;
        [SerializeField] private float maxShotDelay = 2.0f;

        public bool isReloading => _weaponHolder != null && _weaponHolder.IsReloading;

        private WeaponHolder _weaponHolder;
        private Tween _reloadTween;

        public void Initialize(Entity entity)
        {
            _weaponHolder = entity.GetModule<WeaponHolder>();

            if (_weaponHolder == null)
            {
                Debug.LogError($"WeaponHolder not found on {entity.name}");
                return;
            }

            _weaponHolder.OnReloadEvent.AddListener(HandleReloadEvent);
        }

        private void OnDestroy()
        {
            if (_weaponHolder != null)
                _weaponHolder.OnReloadEvent.RemoveListener(HandleReloadEvent);

            _reloadTween?.Kill();
        }

        public void AfterInitialize()
        {
            WeaponDataSO weapon = (WeaponDataSO)weaponItem.Clone();
            if (randomizeShotDelay)
                weapon.shotDelay = Random.Range(minShotDelay, maxShotDelay);
            weaponItem = weapon;
            _weaponHolder.SwapWeapon(weaponItem);
        }

        public void SetAim(Vector2 targetPoint)
        {
            _weaponHolder?.SetAim(targetPoint);
        }

        public void Attack(Transform target)
        {
            if (target == null || _weaponHolder == null)
                return;

            _weaponHolder.SetAim(target.position);
            _weaponHolder.HandleAttack(true);
        }

        public void StartAttack()
        {
            _weaponHolder?.HandleAttack(true);
        }

        public void StopAttack()
        {
            _weaponHolder?.HandleAttack(false);
        }

        public bool CanReload()
        {
            return _weaponHolder != null && _weaponHolder.CanReload();
        }

        public bool NeedReload()
        {
            if (_weaponHolder == null || _weaponHolder.CurrentWeapon == null)
                return false;

            Weapon weapon = _weaponHolder.CurrentWeapon;
            if (weapon.weaponData == null)
                return false;

            if (weapon.IsReloading)
                return false;

            return weapon.weaponData.currentAmmo <= 0;
        }

        public void Reload()
        {
            _weaponHolder?.Reload();
        }

        private void HandleReloadEvent(float reloadTime)
        {
            _reloadTween?.Kill();
            _reloadTween = DOVirtual.DelayedCall(reloadTime, () => { });
        }
    }
}