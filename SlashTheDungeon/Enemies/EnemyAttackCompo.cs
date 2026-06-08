using System;
using HN.Code.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace SJ._01.Code.Enemies
{
    public class EnemyAttackCompo : MonoBehaviour
    {
        public event Action OnAttackEvent;

        [SerializeField] private EnemyDataSO _enemyDataSO;
        [SerializeField] private EnemyAnimatorTrigger _animTrigger;
        [SerializeField] private DamageCaster _damageCaster;

        public bool IsAttacking { get; private set; }

        private float _lastAttackTime = -999f;

        private void Awake()
        {
            if (_animTrigger != null)
            {
                _animTrigger.OnAnimationEndEvent += TurnOffAttacking;
                _animTrigger.OnAttackEvent += HandleAttackTrigger;
            }
        }

        private void OnDestroy()
        {
            if (_animTrigger != null)
            {
                _animTrigger.OnAnimationEndEvent -= TurnOffAttacking;
                _animTrigger.OnAttackEvent -= HandleAttackTrigger;
            }
        }

        public void TryAttack()
        {
            if (IsAttacking) return;
            if (_enemyDataSO == null) return;
            if (_lastAttackTime + _enemyDataSO.attackCooldown > Time.time) return;

            IsAttacking = true;
            _lastAttackTime = Time.time;
            _animTrigger?.PlayAttackAnimation();
        }

        public void TurnOffAttacking()
        {
            IsAttacking = false;
            _animTrigger?.PlayIdleAnimation();
        }

        private void HandleAttackTrigger()
        {
            if (!IsAttacking) return;
            if (_enemyDataSO == null) return;
            OnAttackEvent?.Invoke();
            _damageCaster?.CastDamageOverlapBox(_enemyDataSO.attackDamage);
        }
    }
}