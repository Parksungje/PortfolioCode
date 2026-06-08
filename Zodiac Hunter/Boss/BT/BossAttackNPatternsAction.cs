using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Work.PSJ.Code.Enemies;
using Work.PSJ.Code.Enemies.BT;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Boss.BT
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossAttackNPatterns", story: "[Self] attack [Target] for [PatternCount] patterns then move windup [WindupDuration]", category: "Boss/Action", id: "b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7")]
    public partial class BossAttackNPatternsAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractBoss> Self;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<int> PatternCount;
        [SerializeReference] public BlackboardVariable<float> FallbackDuration;
        [SerializeReference] public BlackboardVariable<float> WindupDuration;

        private IEnemyAttackModule _attackModule;
        private BasePatternAttackCompo _patternCompo;
        private int _cyclesCompleted;
        private float _timer;
        private bool _useFallback;
        private bool _isWindingUp;
        private float _windupTimer;
        private float _windupDuration;
        private bool _attackSubscribed;

        protected override Status OnStart()
        {
            if (Self.Value == null || Target.Value == null)
                return Status.Failure;

            _attackModule = Self.Value.GetComponentInChildren<IEnemyAttackModule>();
            _patternCompo = Self.Value.GetComponentInChildren<BasePatternAttackCompo>();

            if (_attackModule == null)
                return Status.Failure;

            _cyclesCompleted = 0;
            _timer = 0f;
            _useFallback = _patternCompo == null;
            _attackSubscribed = false;

            _windupDuration = WindupDuration != null && WindupDuration.Value > 0f ? WindupDuration.Value : 0f;
            _attackModule.SetAim(Target.Value.position);

            if (_windupDuration > 0f)
            {
                _isWindingUp = true;
                _windupTimer = 0f;
                Self.Value.StartWindupEffect(_windupDuration);
            }
            else
            {
                _isWindingUp = false;
                BeginAttack();
            }

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Self.Value == null || Target.Value == null)
                return Status.Failure;

            _attackModule?.SetAim(Target.Value.position);

            if (_isWindingUp)
            {
                _windupTimer += Time.deltaTime;
                if (_windupTimer >= _windupDuration)
                {
                    _isWindingUp = false;
                    BeginAttack();
                }
                return Status.Running;
            }

            bool done;
            if (_useFallback)
            {
                _timer += Time.deltaTime;
                float duration = FallbackDuration != null && FallbackDuration.Value > 0f ? FallbackDuration.Value : 3f;
                done = _timer >= duration;
            }
            else
            {
                int needed = PatternCount != null && PatternCount.Value > 0 ? PatternCount.Value : 2;
                done = _cyclesCompleted >= needed;
            }

            if (done)
            {
                Self.Value.SetVariableValue(BTVariables.CurrentState, AbstractBoss.BossState.MOVE);
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            Self.Value?.StopWindupEffect();

            if (_attackSubscribed && !_useFallback && _patternCompo != null)
                _patternCompo.OnPatternCycled -= OnPatternCycled;

            _attackModule?.StopAttack();
        }

        private void BeginAttack()
        {
            if (!_useFallback && _patternCompo != null)
            {
                _patternCompo.OnPatternCycled += OnPatternCycled;
                _attackSubscribed = true;
            }
            _attackModule.Attack(Target.Value);
        }

        private void OnPatternCycled() => _cyclesCompleted++;
    }
}