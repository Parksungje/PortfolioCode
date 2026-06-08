using System;
using Unity.Behavior;
using UnityEngine;
using Work.PSJ.Code.Enemies;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CannotReloadCondition", story: "[Self] cannot reload", category: "Conditions", id: "152a844d323357181fc26b3a7aa44a37")]
public partial class CannotReloadCondition : Condition
{
    [SerializeReference] public BlackboardVariable<AbstractEnemy> Self;

    public override bool IsTrue()
    {
        if (Self == null || Self.Value == null)
            return true;

        IEnemyAttackModule attackModule = Self.Value.GetAttackModule();
        if (attackModule == null)
            return true;

        return !attackModule.CanReload();
    }
}
