using UnityEngine;

namespace Work.PSJ.Code.Enemies.BT
{
    public enum BTVariables
    {
        DetectRadius,
        AttackRadius,
        CurrentState,
        Target,
    }
    
    [CreateAssetMenu(fileName = "Variable data", menuName = "SO/BT/Variable", order = 0)]
    public class VariableSO : ScriptableObject
    {
        public BTVariables variableName;
    }
}