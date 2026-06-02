using Code.Combat.Projectiles;
using UnityEngine;

namespace Work.PSJ.Code.Weapons
{
    public class HomingProjectile : Projectile
    {
        private Transform _target;
        private float _homingStrength;
        private float _homingStartTime;

        public void SetupHoming(Transform target, float homingStrength, float homingDelay)
        {
            _target = target;
            _homingStrength = homingStrength;
            _homingStartTime = Time.time + homingDelay;
        }

        protected override void Update()
        {
            base.Update();

            if (_target == null || Time.time < _homingStartTime)
                return;

            Vector2 toTarget = ((Vector2)_target.position - (Vector2)_Trm.position).normalized;
            float speed = _rigid.linearVelocity.magnitude;

            _rigid.linearVelocity = Vector2.Lerp(
                _rigid.linearVelocity.normalized,
                toTarget,
                _homingStrength * Time.deltaTime
            ) * speed;
        }

        public override void ResetItem()
        {
            base.ResetItem();
            _target = null;
            _homingStrength = 0f;
            _homingStartTime = 0f;
        }
    }
}