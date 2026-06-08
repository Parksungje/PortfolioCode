using System.Collections;
using Code.Combat;
using Code.Entities;
using Code.Feedback;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace Work.PSJ.Code.Feedbacks.BossFeedback
{
    public class BossHitFeedback : Feedback, IEntityModule
    {
        [SerializeField] private SpriteRenderer[] targetRenderers;
        [SerializeField] private float hitStopDuration = 0.05f;
        [SerializeField] private float blinkDuration = 0.12f;
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private float hitImpulseForce = 0.4f;

        private static readonly int BlinkHash = Shader.PropertyToID("_Blink");
        private EntityHealthModule _healthModule;
        private bool _isHitStopping;

        public void Initialize(Entity entity)
        {
            _healthModule = entity.GetModule<EntityHealthModule>();
        }

        public override void CreateFeedback()
        {
            if (_healthModule != null)
                _healthModule.OnHitEvent.AddListener(OnHit);
        }

        public override void StopFeedback()
        {
            if (_healthModule != null)
                _healthModule.OnHitEvent.RemoveListener(OnHit);
        }

        private void OnHit(float max, float current)
        {
            TriggerBlink();
            TriggerCameraShake();
            if (!_isHitStopping)
                StartCoroutine(HitStopCoroutine());
        }

        private void TriggerBlink()
        {
            foreach (var sr in targetRenderers)
            {
                if (sr == null) continue;
                sr.material.SetInt(BlinkHash, 1);
                DOVirtual.DelayedCall(blinkDuration, () =>
                {
                    if (sr != null) sr.material.SetInt(BlinkHash, 0);
                }).SetUpdate(true);
            }
        }

        private void TriggerCameraShake()
        {
            if (impulseSource != null)
                impulseSource.GenerateImpulse(hitImpulseForce);
        }

        private IEnumerator HitStopCoroutine()
        {
            _isHitStopping = true;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            Time.timeScale = 1f;
            _isHitStopping = false;
        }
    }
}
