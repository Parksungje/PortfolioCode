using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "MeleeAttackTarget", story: "[Self] attack [Target] [Result]", category: "Action", id: "7b75f6e3db16fd2c97017112e4c9cdfe")]
    public partial class MeleeAttackTargetAction : Action
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
                return Status.Failure;

            if (Result.Value)
            {
                _attackModule.SetAim(Target.Value.position);
                _attackModule.Attack(Target.Value);
            }

            return Status.Success;
        }
    }
}
