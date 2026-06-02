using System;
using Code.Player;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "AttackTarget", story: "[Self] attack [Target] [Result]", category: "Action", id: "cdfcee39a15e53871b6749c3d326eb27")]
    public partial class AttackTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Self;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<bool> Result;

        private IEnemyAttackModule _attackModule;

        protected override Status OnStart()
        {
            if (Self.Value == null || Target.Value == null)
                return Status.Failure;

            _attackModule = Self.Value.GetAttackModule();
            if (_attackModule == null)
            {
                Debug.LogWarning($"{Self.Value.name} AttackModule not found");
                return Status.Failure;
            }

            if (Result.Value)
            {
                _attackModule.SetAim(Target.Value.position);
                _attackModule.Attack(Target.Value);
            }

            return Status.Success;
        }
    }
}