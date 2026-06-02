using Code.Effects;
using Code.Feedback;
using ObjectPool.RunTime;
using UnityEngine;

namespace Work.PSJ.Code.Feedbacks
{
    public class EnemyEffectFeedback : Feedback
    {
        [SerializeField] private PoolManagerSO _poolManager;
        [SerializeField] private PoolingItemSO _deathEffectItem;
        
        public override void CreateFeedback()
        {
            Vector2 center = transform.position;
            
            var item = (PoolingEffect)_poolManager.Pop(_deathEffectItem);
            item.PlayParticle(center);
        }

        public override void StopFeedback()
        {
            
        }
    }
}