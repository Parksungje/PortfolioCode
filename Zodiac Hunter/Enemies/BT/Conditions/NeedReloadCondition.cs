using System;
using Unity.Behavior;
using UnityEngine;

namespace Work.PSJ.Code.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "NeedReloadCondition", story: "[Self] need reload", category: "Conditions", id: "8d8a0c9f4d274a8d9f2fbb0d2a65a111")]
    public partial class NeedReloadCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Self;

        public override bool IsTrue()
        {
            if (Self == null || Self.Value == null)
                return false;

            IEnemyAttackModule attackModule = Self.Value.GetAttackModule();
            if (attackModule == null)
                return false;

            return attackModule.NeedReload();
        }
    }
}