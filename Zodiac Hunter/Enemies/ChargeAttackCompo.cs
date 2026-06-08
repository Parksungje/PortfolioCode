using System.Collections;
using Code.Combat;
using Code.Entities;
using Work.PSJ.Code.Combat;
using UnityEngine;

namespace Work.PSJ.Code.Enemies
{
    public class ChargeAttackCompo : MonoBehaviour, IEntityModule, IAfterInitialize, IEnemyAttackModule
    {
        [SerializeField] private float damage = 2f;
        [SerializeField] private float chargeSpeed = 20f;
        [SerializeField] private float chargeDuration = 0.25f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float damageRadius = 0.8f;
        
        [SerializeField] private float telegraphDuration = 0.5f;
        
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private Animator animator;

        private Entity _entity;
        private EntityMover _mover;
        private ITelegraph _telegraph;
        
        private float _lastAttackTime = -999f;
        private bool _isCharging;

        private readonly int _attackHash = Animator.StringToHash("Attack");
        private readonly int _idleHash = Animator.StringToHash("Idle");

        private readonly Collider2D[] _targetHits = new Collider2D[1];
        private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[1];

        private const float WallCheckRadiusMultiplier = 0.5f;
        private const float PostChargeDelay = 0.2f;

        public bool isReloading => false;

        public void Initialize(Entity entity)
        {
            _entity = entity;
            _mover = entity.GetModule<EntityMover>();
            _telegraph = entity.GetModule<TelegraphCompo>();
        }

        public void Attack(Transform target)
        {
            if (target == null || _isCharging || Time.time < _lastAttackTime + attackCooldown) 
                return;

            StartCoroutine(ChargeRoutine(target));
        }

        private IEnumerator ChargeRoutine(Transform target)
        {
            PrepareCharge(target, out Vector2 chargeDir, out float chargeDistance);

            if (_telegraph != null)
            {
                yield return _telegraph.ShowWarning(transform, chargeDir, chargeDistance, damageRadius * 2f, telegraphDuration);
            }
            else 
            {
                yield return new WaitForSeconds(telegraphDuration);
            }

            animator?.SetTrigger(_attackHash);
            yield return PerformCharge(chargeDir);

            EndCharge();
        }

        private void PrepareCharge(Transform target, out Vector2 chargeDir, out float chargeDistance)
        {
            _isCharging = true;
            _lastAttackTime = Time.time;
            
            chargeDir = (target.position - transform.position).normalized;
            chargeDistance = chargeSpeed * chargeDuration;
            
            _mover?.Stop();
            _mover?.SetManualMove(false);
        }

        private IEnumerator PerformCharge(Vector2 chargeDir)
        {
            float elapsed = 0f;
            bool hasDamaged = false;

            while (elapsed < chargeDuration)
            {
                if (CheckWallCollision(chargeDir))
                    break;

                _mover?.SetVelocity(chargeDir * chargeSpeed);

                if (!hasDamaged && CheckAndApplyDamage())
                {
                    hasDamaged = true;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void EndCharge()
        {
            _mover?.SetVelocity(Vector2.zero);
            _mover?.SetManualMove(true);
            _isCharging = false;
            
            animator?.SetTrigger(_idleHash);
        }

        private bool CheckWallCollision(Vector2 direction)
        {
            int hitCount = Physics2D.CircleCastNonAlloc(
                transform.position, 
                damageRadius * WallCheckRadiusMultiplier, 
                direction, 
                _wallHits, 
                chargeSpeed * Time.deltaTime, 
                wallLayer);
                
            return hitCount > 0;
        }

        private bool CheckAndApplyDamage()
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, damageRadius, _targetHits, targetLayer);
            
            if (hitCount > 0 && _targetHits[0].TryGetComponent(out EntityHealthModule health))
            {
                var damageData = new DamageData { Damage = this.damage }; 
                health.ApplyDamage(damageData, _targetHits[0].transform.position, _entity);
                return true;
            }
            return false;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _isCharging = false;
            _mover?.SetManualMove(true);
            _telegraph?.CancelWarning();
        }

        public void AfterInitialize() { }
        public void SetAim(Vector2 targetPoint) { }
        public void StartAttack() { }
        public void StopAttack() { }
        public void Reload() { }
        public bool CanReload() => false;
        public bool NeedReload() => false;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
}