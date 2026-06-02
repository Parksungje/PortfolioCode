using Code.Feedback;
using DG.Tweening;
using GondrLib.Events;
using UnityEngine;
using Work.PSJ.Code.Boss;
using Work.PSJ.Code.Boss.Events;

namespace Work.PSJ.Code.Feedbacks.BossFeedback
{
    public class BossPhaseColorFeedback : Feedback
    {
        [SerializeField] private SpriteRenderer[] targetRenderers;
        [SerializeField] private Color phase2Color = new Color(1f, 0.6f, 0.6f, 1f);
        [SerializeField] private Color phase3Color = new Color(1f, 0.15f, 0.15f, 1f);
        [SerializeField] private float transitionDuration = 1.5f;

        public override void CreateFeedback()
        {
            Bus<BossPhaseChangedEvent>.onEvent += HandlePhaseChanged;
        }

        public override void StopFeedback()
        {
            Bus<BossPhaseChangedEvent>.onEvent -= HandlePhaseChanged;

            foreach (SpriteRenderer sr in targetRenderers)
            {
                if (sr == null) continue;
                sr.DOKill();
                sr.DOColor(Color.white, transitionDuration).SetEase(Ease.InQuad);
            }
        }

        private void HandlePhaseChanged(BossPhaseChangedEvent evt)
        {
            Color targetColor = evt.Phase switch
            {
                AbstractBoss.BossState.PHASE2 => phase2Color,
                AbstractBoss.BossState.PHASE3 => phase3Color,
                _ => Color.white
            };

            foreach (SpriteRenderer sr in targetRenderers)
            {
                if (sr == null) continue;
                sr.DOKill();
                sr.DOColor(targetColor, transitionDuration).SetEase(Ease.InQuad);
            }
        }
    }
}