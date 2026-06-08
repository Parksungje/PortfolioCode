using CSI._01.Script.Enemy;
using HN.HNLib.ObjectPool;
using UnityEngine;

    public class CaptainSkeletonAttackState : EnemyState
    {
        private bool _isAttacked;
        
        public CaptainSkeletonAttackState(Enemy enemy) : base(enemy)
        {
        }
        protected override void EnterState()
        {
            base.EnterState();
            _enemy.AttackCompo.Attack();
            _enemy.AnimTrigger.OnAnimationEndEvent += AnimationEnd;
            _enemy.AnimTrigger.OnAttackEvent += HandleAttackEvent;
            _enemy.StopnavMesh(true);
            
            CaptainSkeleton captainSkeleton = _enemy as CaptainSkeleton;
            if(captainSkeleton == null) return;
            
            captainSkeleton.attackDir = (_enemy.Player.transform.position - _enemy.transform.position).normalized;
        }

        private void HandleAttackEvent()
        {
            if(_isAttacked) return;
            
            _isAttacked = true;
            
            _enemy.InitSlash();
        }

        private void AnimationEnd()
        {
            _enemy.TransitionState(EnemyStateType.Idle);

            _isAttacked = false;
        }


        protected override void ExtiState()
        {
            _enemy.AnimTrigger.OnAnimationEndEvent -= AnimationEnd;
            _enemy.AnimTrigger.OnAttackEvent -= HandleAttackEvent;
            _enemy.StopnavMesh(false);
            base.ExtiState();
        }
    }
