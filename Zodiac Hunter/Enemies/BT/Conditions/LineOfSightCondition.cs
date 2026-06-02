using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Work.PSJ.Code.Enemies;

[Serializable, GeneratePropertyBag]
[Condition(name: "LineOfSightCondition", story: "[Self] has line of sight to [Target]", category: "Conditions", id: "a3f7b2c1d4e5f6a7b8c9d0e1f2a3b4c5")]
public partial class LineOfSightCondition : Condition
{
    [SerializeReference] public BlackboardVariable<AbstractEnemy> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    public override bool IsTrue()
    {
        if (Self?.Value == null || Target?.Value == null)
            return false;

        if (Self.Value is RangeEnemy rangeEnemy)
            return rangeEnemy.CanSeeTarget(Target.Value);

        return false;
    }
}