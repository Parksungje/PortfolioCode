using Code.Combat;
using Code.Entities;
using DG.Tweening;
using Map;
using UnityEngine;

namespace Work.PSJ.Code.Enemies
{
    public class MeleeEnemyAttackCompo : MonoBehaviour, IEntityModule, IAfterInitialize, IEnemyAttackModule
    {
        [SerializeField] private float damage = 1f;
        [SerializeField] private float attackCooldown = 0.3f;
        [SerializeField] private Transform visualTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private float punchDistance = 0.35f;
        [SerializeField] private float punchDuration = 0.08f;
        [SerializeField] private float returnDuration = 0.12f;
        [SerializeField] private float hitDelay = 0.08f;
        [SerializeField] private float maxHitDistance = 1.35f;

        public bool isReloading => false;

        private Entity _entity;
        private float _lastAttackTime;
        private Vector3 _visualOriginLocalPos;
        private Tween _attackTween;
        private readonly int _attackHash = Animator.StringToHash("Attack");
        private readonly int _idleHash = Animator.StringToHash("Idle");

        public void Initialize(Entity entity)
        {
            _entity = entity;
        }

        public void AfterInitialize()
        {
            if (visualTransform != null)
                _visualOriginLocalPos = visualTransform.localPosition;
        }

        public void SetAim(Vector2 targetPoint)
        {
        }

        public void Attack(Transform target)
        {
            if (target == null)
                return;

            if (Time.time < _lastAttackTime + attackCooldown)
                return;

            EntityHealthModule healthModule = target.GetComponentInParent<EntityHealthModule>();
            if (healthModule == null)
                return;

            _lastAttackTime = Time.time;

            PlayAttackAnimation();
            PlayAttackFeedback(target, healthModule);
        }

        private void PlayAttackAnimation()
        {
            if (animator == null)
                return;

            animator.SetTrigger(_attackHash);
        }

        private void PlayAttackFeedback(Transform target, EntityHealthModule healthModule)
        {
            if (visualTransform == null)
            {
                ApplyDamageIfStillValid(target, healthModule);
                return;
            }

            _attackTween?.Kill();
            visualTransform.localPosition = _visualOriginLocalPos;

            Vector3 dir = (target.position - transform.position).normalized;
            Vector3 localOffset = transform.InverseTransformDirection(dir) * punchDistance;
            localOffset.z = 0f;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(visualTransform.DOLocalMove(_visualOriginLocalPos + localOffset, punchDuration));
            sequence.InsertCallback(hitDelay, () => ApplyDamageIfStillValid(target, healthModule));
            sequence.Append(visualTransform.DOLocalMove(_visualOriginLocalPos, returnDuration));
            sequence.OnComplete(StopAttack);
            _attackTween = sequence;
        }

        private void ApplyDamageIfStillValid(Transform target, EntityHealthModule healthModule)
        {
            if (target == null || healthModule == null)
                return;

            if (Vector2.Distance(transform.position, target.position) > maxHitDistance &&
                !IsTargetReachableByTile(target))
                return;

            DamageData damageData = new DamageData();
            damageData.Damage = damage;
            healthModule.ApplyDamage(damageData, target.position, _entity);
        }

        private bool IsTargetReachableByTile(Transform target)
        {
            if (target == null || MapManager.Instance == null)
                return false;

            Vector3Int selfCell = MapManager.Instance.GetCellPosition(transform.position);
            Vector3Int targetCell = MapManager.Instance.GetCellPosition(target.position);
            Vector3Int delta = selfCell - targetCell;

            return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) <= 1;
        }

        public void StartAttack()
        {
        }

        public void StopAttack()
        {
            if (animator != null)
                animator.SetTrigger(_idleHash);
        }

        public void Reload()
        {
        }

        public bool CanReload()
        {
            return false;
        }

        public bool NeedReload()
        {
            return false;
        }

        private void OnDisable()
        {
            _attackTween?.Kill();

            if (visualTransform != null)
                visualTransform.localPosition = _visualOriginLocalPos;
        }
    }
}
