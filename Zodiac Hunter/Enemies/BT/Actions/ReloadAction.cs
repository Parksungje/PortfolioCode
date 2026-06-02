using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "ReloadAction", story: "[Self] Reload", category: "Action", id: "141b472482303c78144f9f4adda8b98a")]
    public partial class ReloadAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Self;

        private IEnemyAttackModule _attackModule;

        protected override Status OnStart()
        {
            if (Self == null || Self.Value == null)
                return Status.Failure;

            _attackModule = Self.Value.GetAttackModule();
            if (_attackModule == null)
                return Status.Failure;

            if (!_attackModule.CanReload())
                return Status.Failure;

            _attackModule.StopAttack();
            _attackModule.Reload();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (_attackModule == null)
                return Status.Failure;

            if (_attackModule.isReloading)
                return Status.Running;

            return Status.Success;
        }
    }
}