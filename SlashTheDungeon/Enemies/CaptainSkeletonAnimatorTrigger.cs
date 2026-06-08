using System;
using UnityEngine;

namespace SJ._01.Code.Enemies
{
    public class CaptainSkeletonAnimatorTrigger : MonoBehaviour
    {
        public event Action OnAnimationEndEvent;
        public event Action OnRollingEndEvent;
        public event Action OnAttackEvent;

        public void OnAnimationEnd()
        {
            OnAnimationEndEvent?.Invoke();
        }

        public void OnRollingEnd()
        {
            OnRollingEndEvent?.Invoke();
        }

        public void OnAttackTrigger()
        {
            OnAttackEvent?.Invoke();
        }
    }
}