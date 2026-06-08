using CSI._01.Script.Enemy;
using UnityEngine;

public class CaptainSkeletonDieState : EnemyState
{
    public CaptainSkeletonDieState(Enemy enemy) : base(enemy)
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