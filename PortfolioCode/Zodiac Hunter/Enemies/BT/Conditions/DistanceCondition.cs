using System;
using Map;
using Unity.Behavior;
using UnityEngine;
using Work.PSJ.Code.Enemies;
using Work.PSJ.Code.Enemies.BT;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "DistanceCondition", story: "Distance from [Self] to [Target] is [CompareType] [Range]", category: "Conditions", id: "f99e55774d7e4a96b06d3d7776d48394")]
public partial class DistanceCondition : Condition
{
    [SerializeReference] public BlackboardVariable<AbstractEnemy> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<RangeCompareType> CompareType;
    [SerializeReference] public BlackboardVariable<float> Range;

    public override bool IsTrue()
    {
        if (Self == null || Self.Value == null)
            return false;

        if (Target == null || Target.Value == null)
            return false;

        float distance = GetDistance();

        return Compare(distance, Range.Value, CompareType.Value);
    }

    private float GetDistance()
    {
        float distance = Vector2.Distance(
            Self.Value.transform.position,
            Target.Value.position
        );

        if (Self.Value is not MeleeEnemy || MapManager.Instance == null)
            return distance;

        if (!IsSameOrAdjacentCell(Self.Value.transform.position, Target.Value.position))
            return distance;

        float meleeReachDistance = Mathf.Max(0f, Range.Value - 0.01f);
        return Mathf.Min(distance, meleeReachDistance);
    }

    private static bool IsSameOrAdjacentCell(Vector3 selfPosition, Vector3 targetPosition)
    {
        Vector3Int selfCell = MapManager.Instance.GetCellPosition(selfPosition);
        Vector3Int targetCell = MapManager.Instance.GetCellPosition(targetPosition);
        Vector3Int delta = selfCell - targetCell;

        return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) <= 1;
    }

    private bool Compare(float distance, float range, RangeCompareType compareType)
    {
        return compareType switch
        {
            RangeCompareType.LessThan => distance < range,
            RangeCompareType.LessThanOrEqual => distance <= range,
            RangeCompareType.GreaterThan => distance > range,
            RangeCompareType.GreaterThanOrEqual => distance >= range,
            _ => false
        };
    }
}
