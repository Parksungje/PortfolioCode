using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Work.PSJ.Code.Enemies;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Boss.BT
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossAttackTarget", story: "[Self] attack [Target]", category: "Boss/Action", id: "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6")]
    public partial class BossAttackTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractBoss> Self;
        [SerializeReference] public BlackboardVariable<Transform> Target;

        private IEnemyAttackModule _attackModule;

        protected override Status OnStart()
        {
            if (Self.Value == null || Target.Value == null)
                return Status.Failure;

            _attackModule = Self.Value.GetComponentInChildren<IEnemyAttackModule>();
            if (_attackModule == null)
            {
                Debug.LogWarning($"{Self.Value.name} AttackModule not found");
                return Status.Failure;
            }

            _attackModule.SetAim(Target.Value.position);
            _attackModule.Attack(Target.Value);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Self.Value == null || Target.Value == null)
                return Status.Failure;

            _attackModule?.SetAim(Target.Value.position);
            return Status.Running;
        }

        protected override void OnEnd()
        {
            _attackModule?.StopAttack();
        }
    }
}