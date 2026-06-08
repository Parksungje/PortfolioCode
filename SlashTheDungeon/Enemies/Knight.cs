using UnityEngine;

namespace SJ._01.Code.Enemies
{
    public class Knight : Enemy
    {
        [SerializeField] private EnemyAnimatorTrigger _animTrigger;
        [SerializeField] private EnemyAttackCompo _attackCompo;

        protected override void Awake()
        {
            base.Awake();
            if (_animTrigger == null)
                _animTrigger = GetComponent<EnemyAnimatorTrigger>();

            if (_animTrigger != null)
            {
                _animTrigger.OnAnimationEndEvent += OnAnimEnd;
            }
        }

        private void OnDestroy()
        {
            if (_animTrigger != null)
            {
                _animTrigger.OnAnimationEndEvent -= OnAnimEnd;
            }
        }

        protected override void Update()
        {
            base.Update(); // 부모의 경로 추적 로직 실행

            if (enemyDataSO == null || target == null) return;

            float distance = Vector2.Distance(transform.position, target.position);
            if (distance <= enemyDataSO.attackRadius && _attackCompo != null && !_attackCompo.IsAttacking)
            {
                _attackCompo.TryAttack();
            }
        }

        private void OnAnimEnd()
        {
            _animTrigger?.PlayIdleAnimation();
        }

        private void OnDrawGizmosSelected()
        {
            if (enemyDataSO == null) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyDataSO.detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyDataSO.attackRadius);
        }
    }
}