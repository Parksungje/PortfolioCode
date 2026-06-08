using CSI._01.Script.Enemy;
using UnityEngine;

    public class FireBirdMoveState : EnemyState
    {
        public FireBirdMoveState(Enemy enemy) : base(enemy)
        {
        }
        
        public override void UpdateState()
        {
            base.UpdateState();
            _enemy.navMesh.SetDestination(_enemy.Player.transform.position);
            _enemy.AnimTrigger.SetXY(_enemy.navMesh.velocity);
            if (_enemy.navMesh.velocity.x < 0)
            {
                if (!_enemy.IsFlip())
                    _enemy.FlipX(true);
            }
            else if (_enemy.navMesh.velocity.x > 0)
            {
                if (_enemy.IsFlip())
                    _enemy.FlipX(false);
            }

            if (Vector3.Distance(_enemy.transform.position, _enemy.Player.transform.position) <= _enemy.attackRange)
            {
                if (_enemy.AttackCompo.CanAttack())
                {
                    _enemy.TransitionState(EnemyStateType.Attack);
                }
            }
            else if (Vector3.Distance(_enemy.transform.position, _enemy.Player.transform.position) >= _enemy.moveRange)
            {
                _enemy.TransitionState(EnemyStateType.Idle);

            }
        }

        protected override void ExtiState()
        {
            base.ExtiState();
        
        }
    }
