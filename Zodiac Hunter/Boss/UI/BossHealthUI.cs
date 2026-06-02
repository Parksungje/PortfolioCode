using DG.Tweening;
using GondrLib.Events;
using UnityEngine;
using UnityEngine.UI;
using Code.Events;
using Work.CJW.Code.UI;
using Work.PSJ.Code.Boss.Events;

namespace Work.PSJ.Code.Boss.UI
{
    public class BossHealthUI : MonoBehaviour
    {
        [SerializeField] private ProgressBarUI barUI;
        [SerializeField] private GameObject container;

        [Header("Hit Effects")]
        [SerializeField] private Image mainBarImage;
        [SerializeField] private Image ghostBarImage;
        [SerializeField] private Color normalColor = new Color(0.85f, 0.15f, 0.15f);
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private float ghostDelay = 0.25f;
        [SerializeField] private float ghostDrainDuration = 0.45f;

        private Tween _flashTween;
        private Tween _ghostTween;
        private float _previousRatio = 1f;

        private void Awake()
        {
            Bus<BossHealthChangedEvent>.onEvent += HandleHealthChanged;
            Bus<BossPhaseChangedEvent>.onEvent += HandlePhaseChanged;
            Bus<PlayerDeathEvent>.onEvent += HandlePlayerDeath;

            if (container != null)
                container.SetActive(false);

            if (mainBarImage != null)
                mainBarImage.color = normalColor;
        }

        private void OnDestroy()
        {
            Bus<BossHealthChangedEvent>.onEvent -= HandleHealthChanged;
            Bus<BossPhaseChangedEvent>.onEvent -= HandlePhaseChanged;
            Bus<PlayerDeathEvent>.onEvent -= HandlePlayerDeath;

            _flashTween?.Kill();
            _ghostTween?.Kill();
        }

        private void HandleHealthChanged(BossHealthChangedEvent evt)
        {
            if (container != null)
                container.SetActive(true);

            float newRatio = evt.CurrentHealth / evt.MaxHealth;
            barUI.SetProgress(evt.MaxHealth, evt.CurrentHealth);
            PlayHitEffect(newRatio);
        }

        private void HandlePhaseChanged(BossPhaseChangedEvent evt)
        {
            if (evt.Phase == AbstractBoss.BossState.DEAD && container != null)
                container.SetActive(false);
        }

        private void HandlePlayerDeath(PlayerDeathEvent evt)
        {
            _flashTween?.Kill();
            _ghostTween?.Kill();

            if (container != null)
                container.SetActive(false);
        }

        private void PlayHitEffect(float newRatio)
        {
            bool healthIncreased = newRatio > _previousRatio;
            _previousRatio = newRatio;

            if (mainBarImage != null)
            {
                _flashTween?.Kill();
                mainBarImage.color = flashColor;
                _flashTween = mainBarImage.DOColor(normalColor, flashDuration).SetEase(Ease.OutQuad);
            }

            if (ghostBarImage != null)
            {
                _ghostTween?.Kill();

                if (healthIncreased)
                {
                    ghostBarImage.fillAmount = newRatio;
                }
                else
                {
                    _ghostTween = DOVirtual.DelayedCall(ghostDelay, () =>
                        ghostBarImage.DOFillAmount(newRatio, ghostDrainDuration).SetEase(Ease.OutCubic)
                    );
                }
            }
        }
    }
}
