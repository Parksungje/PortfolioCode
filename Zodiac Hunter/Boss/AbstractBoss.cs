using System.Collections;
using System.Collections.Generic;
using Code.Combat;
using Code.Entities;
using Code.Player;
using GondrLib.Events;
using ObjectPool.RunTime;
using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.Events;
using Work.PSJ.Code.Boss.Events;
using Work.PSJ.Code.Enemies.BT;

namespace Work.PSJ.Code.Boss
{
    public abstract class AbstractBoss : LifeEntity
    {
        [BlackboardEnum]
        public enum BossState
        {
            PHASE1,
            PHASE2,
            PHASE3,
            MOVE,
            DEAD
        }

        [field: SerializeField] public PlayerSO Target { get; set; }
        [field: SerializeField] public LayerMask LayerMask { get; set; }
        
        private const int Phase1Index = 1;
        private const int Phase2Index = 2;
        private const int Phase3Index = 3;

        [SerializeField] [Range(Phase1Index, Phase3Index)] private int maxPhaseCount = Phase3Index;
        [SerializeField] [Range(0f, 1f)] private float phase2Threshold = 0.66f;
        [SerializeField] [Range(0f, 1f)] private float phase3Threshold = 0.33f;
        [field: SerializeField] public UnityEvent<BossState> OnPhaseChanged { get; private set; }
        [SerializeField] private float phaseTransitionPause = 0.6f;
        [SerializeField] private SpriteRenderer[] windupRenderers;
        [SerializeField] private VariableSO[] btVariables;

        private static readonly int BlinkHash = Shader.PropertyToID("_Blink");
        private Dictionary<BTVariables, SerializableGUID> _variableDict;
        private BehaviorGraphAgent _graphAgent;
        private Coroutine _windupCoroutine;
        public BossState CurrentPhase => _currentPhase;
        private BossState _currentPhase = BossState.PHASE1;
        private bool _suppressNextHitHealthEvent;
        protected EntityHealthModule healthModule;

        protected override void Awake()
        {
            base.Awake();

            _graphAgent = GetComponent<BehaviorGraphAgent>();
            healthModule = GetModule<EntityHealthModule>();

            if (healthModule != null)
            {
                healthModule.OnDeathEvent.AddListener(HandleDead);
                healthModule.OnHitEvent.AddListener(CheckPhaseTransition);
                healthModule.OnHitEvent.AddListener(HandleHealthChanged);
            }
        }

        protected virtual void OnDestroy()
        {
            if (healthModule != null)
            {
                healthModule.OnDeathEvent.RemoveListener(HandleDead);
                healthModule.OnHitEvent.RemoveListener(CheckPhaseTransition);
                healthModule.OnHitEvent.RemoveListener(HandleHealthChanged);
            }
        }

        private void HandleHealthChanged(float max, float current)
        {
            if (_suppressNextHitHealthEvent)
            {
                _suppressNextHitHealthEvent = false;
                return;
            }

            Bus<BossHealthChangedEvent>.Raise(new BossHealthChangedEvent(max, current));
        }

        private void Start()        
        {
            InitializeBehaviorVariables();
            BindTargetVariable();
            _currentPhase = BossState.PHASE1;
            SetVariableValue(BTVariables.CurrentState, BossState.PHASE1);
            RaiseHealthChangedEvent();
        }

        private void CheckPhaseTransition(float max, float current)
        {
            if (max <= 0f)
                return;

            if (!TryGetNextPhase(current / max, out BossState nextPhase))
                return;

            TransitionToPhase(nextPhase);
        }

        private bool TryGetNextPhase(float ratio, out BossState nextPhase)
        {
            if (_currentPhase == BossState.PHASE1
                && CanEnterPhase(BossState.PHASE2)
                && ratio <= phase2Threshold)
            {
                nextPhase = BossState.PHASE2;
                return true;
            }

            if (_currentPhase == BossState.PHASE2
                && CanEnterPhase(BossState.PHASE3)
                && ratio <= phase3Threshold)
            {
                nextPhase = BossState.PHASE3;
                return true;
            }

            nextPhase = _currentPhase;
            return false;
        }

        private bool CanEnterPhase(BossState phase)
        {
            return GetPhaseIndex(phase) <= maxPhaseCount;
        }

        private int GetPhaseIndex(BossState phase)
        {
            return phase switch
            {
                BossState.PHASE1 => Phase1Index,
                BossState.PHASE2 => Phase2Index,
                BossState.PHASE3 => Phase3Index,
                _ => int.MaxValue
            };
        }

        private void TransitionToPhase(BossState phase)
        {
            if (_currentPhase == phase || healthModule == null)
                return;

            _currentPhase = phase;
            healthModule.ResetHealth();
            healthModule.SetCanHit(false);
            _suppressNextHitHealthEvent = true;
            SetVariableValue(BTVariables.CurrentState, phase);
            OnPhaseChanged?.Invoke(phase);
            Bus<BossPhaseChangedEvent>.Raise(new BossPhaseChangedEvent(phase));
            RaiseHealthChangedEvent();
            StartCoroutine(PhaseTransitionCoroutine());
        }

        private IEnumerator PhaseTransitionCoroutine()
        {
            yield return new WaitForSeconds(phaseTransitionPause);
            if (healthModule != null)
                healthModule.SetCanHit(true);
        }

        private void InitializeBehaviorVariables()
        {
            _variableDict = new Dictionary<BTVariables, SerializableGUID>();

            if (_graphAgent == null || btVariables == null)
                return;

            foreach (VariableSO variable in btVariables)
            {
                if (variable == null)
                    continue;

                if (!_graphAgent.GetVariableID(variable.variableName.ToString(), out var guid))
                    continue;

                _variableDict[variable.variableName] = guid;
            }
        }

        private void BindTargetVariable()
        {
            if (Target == null || Target.player == null)
                return;

            SetVariableValue(BTVariables.Target, Target.player.transform);
        }

        private void RaiseHealthChangedEvent()
        {
            if (healthModule == null)
                return;

            Bus<BossHealthChangedEvent>.Raise(
                new BossHealthChangedEvent(healthModule.GetMaxHealth(), healthModule.GetCurrentHealth()));
        }

        public void StartWindupEffect(float duration, float blinkRate = 0.12f)
        {
            StopWindupEffect();
            _windupCoroutine = StartCoroutine(WindupBlinkCoroutine(duration, blinkRate));
        }

        public void StopWindupEffect()
        {
            if (_windupCoroutine != null)
            {
                StopCoroutine(_windupCoroutine);
                _windupCoroutine = null;
            }
            if (windupRenderers == null) return;
            foreach (var sr in windupRenderers)
                if (sr != null) sr.material.SetInt(BlinkHash, 0);
        }

        private IEnumerator WindupBlinkCoroutine(float duration, float blinkRate)
        {
            if (windupRenderers == null || windupRenderers.Length == 0) yield break;

            float elapsed = 0f;
            bool blinkOn = true;
            float blinkTimer = 0f;
            foreach (var sr in windupRenderers)
                if (sr != null) sr.material.SetInt(BlinkHash, 1);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkRate)
                {
                    blinkTimer = 0f;
                    blinkOn = !blinkOn;
                    foreach (var sr in windupRenderers)
                        if (sr != null) sr.material.SetInt(BlinkHash, blinkOn ? 1 : 0);
                }
                yield return null;
            }

            foreach (var sr in windupRenderers)
                if (sr != null) sr.material.SetInt(BlinkHash, 0);
        }

        public void SetVariableValue<T>(BTVariables variable, T value)
        {
            if (_variableDict != null && _variableDict.TryGetValue(variable, out SerializableGUID guid))
                _graphAgent.SetVariableValue(guid, value);
        }

        private void HandleDead()
        {
            SetVariableValue(BTVariables.CurrentState, BossState.DEAD);
            OnDead();
        }

        public virtual void OnDead()
        {
            Destroy(gameObject);
        }
    }
}
