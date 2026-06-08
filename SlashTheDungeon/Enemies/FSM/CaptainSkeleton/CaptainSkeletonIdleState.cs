using CSI._01.Script.Enemy;
using UnityEngine;

    public class CaptainSkeletonIdleState : EnemyState
    {
        public CaptainSkeletonIdleState(Enemy enemy) : base(enemy)
        {
        }
        protected override void EnterState()
        {
            base.EnterState();
            _enemy.StopnavMesh(true);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if(_enemy.Player == null)
            {
                return;
            }
            if (Vector3.Distance(_enemy.transform.position, _enemy.Player.transform.position) <= _enemy.attackRange)
            {
                if (_enemy.AttackCompo.CanAttack())
                {
                    _enemy.TransitionState(EnemyStateType.Attack);
                }
                else
                {
                
                }
            }
            else if (Vector3.Distance(_enemy.transform.position, _enemy.Player.transform.position) <= _enemy.moveRange)
            {
                _enemy.TransitionState(EnemyStateType.Move);
            }
         
        }

        protected override void ExtiState()
        {
            base.ExtiState();
            _enemy.StopnavMesh(false);
        }
    }
