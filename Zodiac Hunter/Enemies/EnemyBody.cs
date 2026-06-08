using ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.Events;
using IPoolable = ObjectPool.RunTime.IPoolable;

namespace Work.PSJ.Code.Enemies
{
    public class EnemyBody : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        public UnityEvent OnSmokeEffect;

        [field: SerializeField] public PoolingItemSO PoolType { get; private set; }
        public GameObject GameObject => gameObject;
        
        protected Pool _myPool;
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            
        }

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
            OnSmokeEffect?.Invoke();
        }
    }
}