using CSI._01.Script.Enemy;
using UnityEngine;


    public class FireBirdDieState : EnemyState
    {
        public FireBirdDieState(Enemy enemy) : base(enemy)
        {
        }
        protected override void EnterState()
        {
            base.EnterState();
            _enemy.StopnavMesh(true);
        }

        protected override void ExtiState()
        {
            base.ExtiState();
        
        }
    }
