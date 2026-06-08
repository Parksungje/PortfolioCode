using Code.Feedback;
using DG.Tweening;
using GondrLib.Events;
using Unity.Cinemachine;
using UnityEngine;
using Work.PSJ.Code.Boss;
using Work.PSJ.Code.Boss.Events;

namespace Work.PSJ.Code.Feedbacks.BossFeedback
{
    public class BossPhaseTransitionFeedback : Feedback
    {
        [SerializeField] private Transform bossTransform;
        [SerializeField] private SpriteRenderer[] targetRenderers;
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private float impulseForce = 3f;
        [SerializeField] private float punchAmount = 0.35f;
        [SerializeField] private float punchDuration = 0.55f;
        [SerializeField] private float flashDuration = 0.1f;

        private static readonly int BlinkHash = Shader.PropertyToID("_Blink");

        public override void CreateFeedback()
        {
            Bus<BossPhaseChangedEvent>.onEvent += HandlePhaseChanged;
        }

        public override void StopFeedback()
        {
            Bus<BossPhaseChangedEvent>.onEvent -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(BossPhaseChangedEvent evt)
        {
            if (evt.Phase == AbstractBoss.BossState.DEAD)
                return;

            if (impulseSource != null)
                impulseSource.GenerateImpulse(impulseForce);

            if (bossTransform != null)
            {
                bossTransform.DOKill();
                bossTransform.DOPunchScale(Vector3.one * punchAmount, punchDuration, 6, 0.5f);
            }

            foreach (var sr in targetRenderers)
            {
                if (sr == null) continue;
                sr.material.SetInt(BlinkHash, 1);
                DOVirtual.DelayedCall(flashDuration, () =>
                {
                    if (sr != null) sr.material.SetInt(BlinkHash, 0);
                });
            }
        }
    }
}
