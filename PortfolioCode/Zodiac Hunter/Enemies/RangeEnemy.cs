using UnityEngine;
using Work.PSJ.Code.Enemies.BT;

namespace Work.PSJ.Code.Enemies
{
    public class RangeEnemy : AbstractEnemy
    {
        [SerializeField] private LayerMask obstacleLayer;

        private IEnemyAttackModule _attackModule;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected void Update()
        {
            if (!IsTargetValid())
                return;

            _attackModule ??= GetAttackModule();
            _attackModule?.SetAim(Target.player.transform.position);

            if (!CanSeePlayer())
            {
                _attackModule?.StopAttack();
                SetVariableValue(BTVariables.Target, Target.player.transform);
                SetVariableValue(BTVariables.CurrentState, EnemyState.CHASE);
            }
            else
            {
                SetVariableValue(BTVariables.Target, Target.player.transform);
            }
        }

        private bool IsTargetValid()
        {
            return Target != null && Target.player != null;
        }

        private bool CanSeePlayer() => CanSeeTarget(Target.player.transform);

        public bool CanSeeTarget(Transform target)
        {
            if (target == null) return false;

            Vector2 origin = transform.position;
            Vector2 direction = (target.position - transform.position).normalized;
            float distance = DetectRadius > 0f ? DetectRadius : 5f;

            int targetLayer = target.gameObject.layer;
            int mask = obstacleLayer | (1 << targetLayer);

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, mask);

            if (hit.collider == null)
                return false;

            return hit.transform == target;
        }

        private void OnDrawGizmos()
        {
            if (!IsTargetValid())
                return;

            Vector2 dir = (Target.player.transform.position - transform.position).normalized;
            float distance = DetectRadius > 0f ? DetectRadius : 5f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)dir * distance);
        }
    }
}
