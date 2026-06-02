using UnityEngine;

namespace Work.PSJ.Code.Enemies
{
    public class TowerEnemy : AbstractEnemy
    {
        protected override EnemyState GetInitState() => EnemyState.ATTACK;
    }
}