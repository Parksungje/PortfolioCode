using UnityEngine;

namespace Work.PSJ.Code.Enemies
{
    public interface IEnemyAttackModule
    {
        void SetAim(Vector2 targetPoint);
        void Attack(Transform target);
        void StopAttack();
        void Reload();
        bool CanReload();
        bool NeedReload();
        bool isReloading { get; }
    }
}