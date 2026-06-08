using System.Collections;
using CSI._01.Script.Enemy;
using UnityEngine;

public class FireBirdAttackState : EnemyState
{
    private bool _isAttacked;
    
    public FireBirdAttackState(CSI._01.Script.Enemy.Enemy enemy) : base(enemy)
    {
    }

    protected override void EnterState()
    {
        base.EnterState();
        _enemy.StopnavMesh(true);
        _enemy.AttackCompo.Attack();
        _enemy.AnimTrigger.OnAnimationEndEvent += AnimationEnd;
        _enemy.AnimTrigger.OnAttackEvent += HandleAttackEvent;
        
        FireBird fireBird = _enemy as FireBird;
        
        if(fireBird == null) return;

        fireBird.attackDir = (_enemy.Player.transform.position - _enemy.transform.position).normalized;
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