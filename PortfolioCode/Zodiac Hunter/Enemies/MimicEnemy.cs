using System.Collections;
using Code.Combat;
using Code.Player;
using UnityEngine;
using Work.PSJ.Code.Enemies.BT;

namespace Work.PSJ.Code.Enemies
{
    public class MimicEnemy : AbstractEnemy
    {
        [SerializeField] private PlayerInputSO playerInput;
        [SerializeField] private float interactDistance = 2f;
        [SerializeField] private Animator animator;
        [SerializeField] private float openDelay = 0.4f;
        [SerializeField] private float waddleFrequency = 8f;
        [SerializeField] private float waddleAngle = 15f;

        private bool _isAwakened;
        private bool _isOpening;

        private readonly int _openHash = Animator.StringToHash("Open");

        protected override void Awake()
        {
            base.Awake();
            healthModule = GetModule<EntityHealthModule>();
        }

        protected override EnemyState GetInitState() => EnemyState.SLEEP;

        private void OnEnable()
        {
            if (playerInput != null)
                playerInput.OnInteractionPressed += HandleInteraction;

            if (healthModule != null)
                healthModule.OnHitEvent.AddListener(HandleHit);
        }

        protected void Update()
        {
            if (!_isAwakened || animator == null) return;

            float angle = Mathf.Sin(Time.time * waddleFrequency) * waddleAngle;
            animator.transform.localEulerAngles = new Vector3(0f, 0f, angle);
        }

        private void OnDisable()
        {
            if (playerInput != null)
                playerInput.OnInteractionPressed -= HandleInteraction;

            if (healthModule != null)
                healthModule.OnHitEvent.RemoveListener(HandleHit);
        }

        private void HandleInteraction()
        {
            if (_isAwakened || _isOpening)
                return;

            if (Target == null || Target.player == null)
                return;

            float distance = Vector2.Distance(transform.position, Target.player.transform.position);
            if (distance > interactDistance)
                return;
            

            StartCoroutine(OpenRoutine());
        }

        private void HandleHit(float maxHealth, float currentHealth)
        {
            if (_isAwakened || _isOpening)
                return;

            StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            _isOpening = true;

            if (Target != null && Target.player != null)
                SetVariableValue(BTVariables.Target, Target.player.transform);

            if (animator != null)
            {
                animator.SetTrigger(_openHash);
            }

            yield return new WaitForSeconds(openDelay);

            _isAwakened = true;
            _isOpening = false;


            SetVariableValue(BTVariables.CurrentState, EnemyState.CHASE);
        }

        public override void ResetItem()
        {
            base.ResetItem();

            SetVariableValue(BTVariables.CurrentState, EnemyState.SLEEP);
            _isAwakened = false;
            _isOpening = false;

            if (animator != null)
            {
                animator.ResetTrigger(_openHash);
                animator.transform.localRotation = Quaternion.identity;
            }
        }
    }
}