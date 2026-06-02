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
    [NodeDescription(name: "MoveToTarget", story: "[Self] Move to [Target]", category: "Action", id: "a62aa6e9a1bbee6120c71d370c1e672c")]
    public partial class MoveToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Self;
        [SerializeReference] public BlackboardVariable<Transform> Target;

        private const float TargetMoveThreshold = 0.8f;
        private const float MinRepathInterval = 0.25f;
        private const float MaxRepathInterval = 0.45f;
        private const float MinPersonalSpace = 0.35f;
        private const float MaxPersonalSpace = 1.1f;

        private PathMover _pathMover;
        private Vector3 _targetPosition;
        private float _nextRepathTime;
        private float _slotAngle;

        protected override Status OnStart()
        {
            if (!TryGetContext(out var self, out _))
                return Status.Failure;

            _pathMover = self.GetModule<PathMover>();
            if (_pathMover == null)
                return Status.Failure;

            _slotAngle = GetStableAngle(self.GetInstanceID());
            UpdateDestination(force: true);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!TryGetContext(out _, out _))
                return Status.Failure;

            UpdateDestination();
            return Status.Success;
        }

        private bool TryGetContext(out AbstractEnemy self, out Transform target)
        {
            self = Self?.Value;
            target = Target?.Value;
            return self != null && target != null;
        }

        private void UpdateDestination(bool force = false)
        {
            if (!force && Time.time < _nextRepathTime) return;

            Vector3 nextPos = GetTargetPosition();
            if (force || Vector2.Distance(_targetPosition, nextPos) > TargetMoveThreshold)
            {
                _targetPosition = nextPos;
                _pathMover.SetDestination(nextPos);
            }

            _nextRepathTime = Time.time + UnityEngine.Random.Range(MinRepathInterval, MaxRepathInterval);
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 targetPos = Target.Value.position;
            float attackRadius = Self.Value.AttackRadius;

            if (attackRadius <= MinPersonalSpace)
                return targetPos;

            float radius = Mathf.Clamp(attackRadius * 0.55f, MinPersonalSpace, MaxPersonalSpace);
            Vector2 offset = new Vector2(Mathf.Cos(_slotAngle), Mathf.Sin(_slotAngle)) * radius;
            return targetPos + new Vector3(offset.x, offset.y, 0f);
        }

        private static float GetStableAngle(int seed)
        {
            unchecked
            {
                uint v = (uint)seed;
                v ^= v >> 16; v *= 0x7feb352dU;
                v ^= v >> 15; v *= 0x846ca68bU;
                v ^= v >> 16;
                return (v / (float)uint.MaxValue) * Mathf.PI * 2f;
            }
        }
    }
}