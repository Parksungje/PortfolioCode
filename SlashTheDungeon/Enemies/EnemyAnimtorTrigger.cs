using System;
using UnityEngine;

namespace SJ._01.Code.Enemies
{
    public class EnemyAnimatorTrigger : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        public event Action OnAttackEvent;
        public event Action OnAnimationEndEvent;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        public void SetXY(Vector2 direction)
        {
            direction.Normalize();
            animator.SetFloat("X", direction.x);
            animator.SetFloat("Y", direction.y);
            //이거 쓰는 법
            // Vector3 dir = (player.position - transform.position).normalized;
            // enemyAnimatorTrigger.SetXY(dir);
        }

        public void PlayAttackAnimation()
        {
            animator.SetTrigger("ATTACK");
        }
        
        public void PlayMoveAnimation()
        {
            animator.SetTrigger("MOVE");
        }

        public void PlayIdleAnimation()
        {
            animator.SetTrigger("IDLE");
        }
        
        public void PlayDeathAnimation()
        {
            animator.SetTrigger("DEATH");
        }

        public void AnimationEnd()
        {
            OnAnimationEndEvent?.Invoke();
        }

        public void AttackTrigger()
        {
            OnAttackEvent?.Invoke();
        }
    }
}