using System;
using Code.Entities;
using Modules;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "PostAttackWander", story: "[Self] wander after attack for [MinDelay] to [MaxDelay] sec within [WanderRadius] at [WanderSpeedScale] speed", category: "Action", id: "3a1f8c2e9b4d7f6e1c2a0b5d8e3f7a9c")]
    public partial class PostAttackWanderAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Self;
        [SerializeReference] public BlackboardVariable<float> MinDelay;
        [SerializeReference] public BlackboardVariable<float> MaxDelay;
        [SerializeReference] public BlackboardVariable<float> WanderRadius;
        [SerializeReference] public BlackboardVariable<float> WanderSpeedScale;

        private float _timer;
        private float _duration;
        private PathMover _pathMover;
        private EntityMover _entityMover;

        protected override Status OnStart()
        {
            if (Self.Value == null)
                return Status.Failure;

            _pathMover = Self.Value.GetModule<PathMover>();
            if (_pathMover == null)
                return Status.Failure;

            _entityMover = Self.Value.GetModule<EntityMover>();

            Self.Value.GetAttackModule()?.StopAttack();

            float speedScale = WanderSpeedScale != null && WanderSpeedScale.Value > 0f ? WanderSpeedScale.Value : 0.45f;
            _entityMover?.SetSpeedMultiplier(speedScale);

            float min = MinDelay.Value > 0 ? MinDelay.Value : 1f;
            float max = MaxDelay.Value > min ? MaxDelay.Value : min + 1f;
            _duration = UnityEngine.Random.Range(min, max);
            _timer = 0f;

            Self.Value.SetVariableValue(BTVariables.CurrentState, AbstractEnemy.EnemyState.WANDER);

            _pathMover.OnMoveEnd += MoveToNextRandomPoint;
            MoveToNextRandomPoint();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            _timer += Time.deltaTime;

            if (_timer >= _duration)
            {
                Self.Value.SetVariableValue(BTVariables.CurrentState, AbstractEnemy.EnemyState.CHASE);
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            _entityMover?.SetSpeedMultiplier(1f);

            if (_pathMover != null)
            {
                _pathMover.OnMoveEnd -= MoveToNextRandomPoint;
                _pathMover.StopMove();
            }

            if (Self.Value != null)
                Self.Value.SetVariableValue(BTVariables.CurrentState, AbstractEnemy.EnemyState.CHASE);
        }

        private void MoveToNextRandomPoint()
        {
            if (Self.Value == null || _pathMover == null) return;

            float radius = WanderRadius.Value > 0 ? WanderRadius.Value : 3f;
            Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 destination = Self.Value.transform.position + new Vector3(offset.x, offset.y, 0f);
            _pathMover.SetDestination(destination);
        }
    }
}
