using System.Collections;
using Code.Combat;
using Code.Combat.Weapons;
using Code.Entities;
using UnityEngine;
using Work.PSJ.Code.Weapons;

namespace Work.PSJ.Code.Enemies
{
    public abstract class BasePatternAttackCompo : MonoBehaviour, IEntityModule, IAfterInitialize, IEnemyAttackModule
    {
        [SerializeField] protected float patternChangeInterval = 2f;
        [SerializeField] private bool randomPattern;

        public bool isReloading => false;

        protected WeaponHolder WeaponHolder { get; private set; }

        private Coroutine _patternCoroutine;
        private Transform _currentTarget;
        private bool _isAttacking;
        private int _currentWeaponIndex = -1;

        protected bool IsAttacking => _isAttacking;
        protected Transform CurrentTarget => _currentTarget;
        public bool IsAttackActive => _isAttacking;
        public Transform AttackTarget => _currentTarget;

        protected abstract WeaponDataSO[] GetActiveWeapons();
        protected virtual float GetCurrentInterval() => patternChangeInterval;

        public virtual void Initialize(Entity entity)
        {
            WeaponHolder = entity.GetModule<WeaponHolder>();

            if (WeaponHolder == null)
                Debug.LogError($"WeaponHolder not found on {entity.name}");
        }

        public virtual void AfterInitialize()
        {
            EquipWeapon(0);
        }

        public void SetAim(Vector2 targetPoint) => WeaponHolder?.SetAim(targetPoint);

        public void Attack(Transform target)
        {
            var weapons = GetActiveWeapons();
            if (WeaponHolder == null || weapons == null || weapons.Length == 0)
                return;

            _currentTarget = target;
            if (_currentTarget != null)
                WeaponHolder.SetAim(_currentTarget.position);

            if (_isAttacking)
                return;

            _isAttacking = true;
            WeaponHolder.HandleAttack(true);

            if (weapons.Length > 1)
                _patternCoroutine = StartCoroutine(PatternLoop());
        }

        public void StopAttack()
        {
            if (WeaponHolder == null)
                return;

            _isAttacking = false;
            _currentTarget = null;

            if (_patternCoroutine != null)
            {
                StopCoroutine(_patternCoroutine);
                _patternCoroutine = null;
            }

            WeaponHolder.HandleAttack(false);
        }

        protected void EquipWeapon(int index)
        {
            var weapons = GetActiveWeapons();
            if (WeaponHolder == null || weapons == null || weapons.Length == 0)
                return;

            index = Mathf.Clamp(index, 0, weapons.Length - 1);

            var cloned = weapons[index].Clone() as WeaponDataSO;
            if (cloned == null)
                return;

            cloned.currentAmmo = int.MaxValue;

            if (cloned is RadialWeaponDataSO radial)
                radial.runtimeAngle = 0f;

            _currentWeaponIndex = index;
            WeaponHolder.SwapWeapon(cloned);
        }

        protected void ResetWeaponIndex() => _currentWeaponIndex = -1;

        public event System.Action OnPatternCycled;

        private IEnumerator PatternLoop()
        {
            while (_isAttacking)
            {
                yield return new WaitForSeconds(GetCurrentInterval());

                if (!_isAttacking)
                    yield break;

                if (_currentTarget != null)
                    WeaponHolder.SetAim(_currentTarget.position);

                EquipWeapon(GetNextPatternIndex());

                WeaponHolder.HandleAttack(false);
                WeaponHolder.HandleAttack(true);

                OnPatternCycled?.Invoke();
            }
        }

        private int GetNextPatternIndex()
        {
            var weapons = GetActiveWeapons();
            if (weapons == null || weapons.Length <= 1)
                return 0;

            if (randomPattern)
            {
                int next = _currentWeaponIndex;
                while (next == _currentWeaponIndex)
                    next = Random.Range(0, weapons.Length);
                return next;
            }

            return (_currentWeaponIndex + 1) % weapons.Length;
        }

        public bool CanReload() => false;
        public bool NeedReload() => false;
        public void Reload() { }
    }
}
