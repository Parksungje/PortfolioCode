using Code.Combat.Weapons;
using UnityEngine;

namespace Work.PSJ.Code.Weapons
{
    [CreateAssetMenu(fileName = "RadialWeaponDataSO", menuName = "SO/SJ/RadialWeaponDataSO")]
    public class RadialWeaponDataSO : WeaponDataSO
    {
        public enum PatternType
        {
            Circle,
            Fan,
            Spiral,
            DoubleSpiral,
            TripleSpiral, 
            RandomBurst,  
        }

        public PatternType patternType = PatternType.Circle;
        public int bulletCount = 8;
        public float startAngleOffset = 0f;
        public float fanAngle = 90f;
        public float spiralStepAngle = 10f;

        [HideInInspector] public float runtimeAngle = 0f;
    }
}