using System.Collections.Generic;
using Code.Combat;
using Modules;
using Code.Entities;
using Code.Player;
using GondrLib.Events;
using ObjectPool.RunTime;
using DG.Tweening;
using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using Work.JES._01.Scripts.Events;
using Work.PSJ.Code.Enemies.BT;

namespace Work.PSJ.Code.Enemies
{
    public abstract class AbstractEnemy : LifeEntity, IPoolable
    {
        [BlackboardEnum]
        public enum EnemyState
        {
            SLEEP,
            IDLE,
            CHASE,
            ATTACK,
            WANDER,
            STUN,
            DEAD
        }

        [field: SerializeField] public float DetectRadius { get; private set; }
        [field: SerializeField] public float AttackRadius { get; private set; }
        [field: SerializeField] public LayerMask TargetLayer { get; private set; }
        [field: SerializeField] public PlayerSO Target { get; private set; }

        [SerializeField] private VariableSO[] btVariables;
        [SerializeField] private PoolManagerSO _poolManager;
        [SerializeField] private Sprite deadSprite;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private PoolingItemSO _bodyPoolingItem;

        private Dictionary<BTVariables, SerializableGUID> _variableDict;
        private BehaviorGraphAgent _graphAgent;
        protected EntityHealthModule healthModule;

        private bool _isSlowed;

        protected override void Awake()
        {
            base.Awake();

            _graphAgent = GetComponent<BehaviorGraphAgent>();
            healthModule = GetModule<EntityHealthModule>();

            if (healthModule != null)
                healthModule.OnDeathEvent.AddListener(HandleDead);

        }

        protected virtual void OnDestroy()
        {
            if (healthModule != null)
                healthModule.OnDeathEvent.RemoveListener(HandleDead);

        }

        private void Start()
        {
            _variableDict = new Dictionary<BTVariables, SerializableGUID>();

            foreach (VariableSO variable in btVariables)
            {
                if (!_graphAgent.GetVariableID(variable.variableName.ToString(), out var guid))
                    continue;

                _variableDict[variable.variableName] = guid;
            }

            SetVariableValue(BTVariables.DetectRadius, DetectRadius);
            SetVariableValue(BTVariables.AttackRadius, AttackRadius);

            if (Target != null && Target.player != null)
                SetVariableValue(BTVariables.Target, Target.player.transform);

            SetVariableValue(BTVariables.CurrentState, GetInitState());
        }

        private void FixedUpdate()
        {
            if (!Target || !Target.player)
                return;

            Vector3 direction = Target.player.transform.position - transform.position;
            SpriteFlip(direction.x < 0);
        }

        protected virtual EnemyState GetInitState() => EnemyState.CHASE;

        public IEnemyAttackModule GetAttackModule() => GetComponentInChildren<IEnemyAttackModule>();

        public void SetVariableValue<T>(BTVariables variable, T value)
        {
            if (_variableDict != null && _variableDict.TryGetValue(variable, out SerializableGUID guid))
                _graphAgent.SetVariableValue(guid, value);
        }
        
        private void SpriteFlip(bool isFlip)
        {
            if (_spriteRenderer)
                _spriteRenderer.flipX = isFlip;
        }

        private void HandleFreezeStart()
        {
            SetVariableValue(BTVariables.CurrentState, EnemyState.STUN);
        }

        private void HandleFreezeEnd()
        {
            SetVariableValue(BTVariables.CurrentState, EnemyState.CHASE);
        }

        private void HandleDead()
        {
            SetVariableValue(BTVariables.CurrentState, EnemyState.DEAD);
            Bus<EnemyDeadEvent>.Raise(new EnemyDeadEvent(this));
            OnDead();
        }

        public virtual void OnDead()
        {
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            var body = (EnemyBody)_poolManager.Pop(_bodyPoolingItem);
            body.transform.position = transform.position;
            body.transform.eulerAngles = new Vector3(0f, 0f, randomAngle);
            body.SetSprite(deadSprite);

            if (_myPool != null)
                _myPool.Push(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, DetectRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRadius);
        }

        [field: SerializeField] public PoolingItemSO PoolType { get; private set; }
        public GameObject GameObject => gameObject;

        protected Pool _myPool;

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public virtual void ResetItem()
        {
            GetModule<PathMover>()?.StopMove();

            if (healthModule)
                healthModule.ResetHealth();

            SetVariableValue(BTVariables.CurrentState, GetInitState());
        }
    }
}