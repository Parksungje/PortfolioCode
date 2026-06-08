using UnityEngine;

namespace SJ._01.Code.Enemies
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "SO/Enemy/EnemyData", order = 0)]
    public class EnemyDataSO : ScriptableObject
    {
        public float speed;
        public float detectionRadius;
        public float attackRadius;
        public float attackCooldown;
        public int maxHealth;
        public int attackDamage;
    }
}