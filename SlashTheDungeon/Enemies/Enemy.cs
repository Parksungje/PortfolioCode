using UnityEngine;
using System;
using HN.Code.Entities;

namespace SJ._01.Code.Enemies
{
    public abstract class Enemy : Entity, IDamageable
    {
        [SerializeField] public EnemyDataSO enemyDataSO;
        public Transform target;

        protected int currentHealth;
        protected bool isDead;

        private int _pathIndex;
        private AstarManager _astarManager;
        private Vector2 _lastTargetPos;
        private EnemyFlip _enemyFlip;

        protected override void Awake()
        {
            base.Awake();
            if (_astarManager == null)
                _astarManager = FindObjectOfType<AstarManager>();
            if (enemyDataSO == null)
                Debug.Log("EnemyDataSO is null.", this);
            else
                currentHealth = enemyDataSO.maxHealth;

            _enemyFlip = GetComponent<EnemyFlip>();

            if (_astarManager != null)
            {
                _astarManager.enemy = this.transform;
                _astarManager.player = target;
            }
        }

        public void Initialize(EnemyDataSO data, AstarManager aManager)
        {
            _astarManager = aManager;
            enemyDataSO = data;
            currentHealth = enemyDataSO.maxHealth;
            if (_astarManager != null)
            {
                _astarManager.enemy = this.transform;
                _astarManager.player = target;
            }
        }

        protected virtual void Update()
        {
            if (enemyDataSO == null || target == null || isDead) return;

            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if ((Vector2)target.position != _lastTargetPos)
            {
                if (_astarManager != null)
                {
                    _astarManager.enemy = this.transform;
                    _astarManager.player = target;
                    _astarManager.PathFinding(
                        Vector2Int.RoundToInt(transform.position),
                        Vector2Int.RoundToInt(target.position)
                    );
                    ResetPathIndex();
                }
                _lastTargetPos = target.position;
            }

            if (distanceToTarget <= enemyDataSO.detectionRadius)
            {
                FollowPath();
            }
        }

        protected void FollowPath()
        {
            if (_astarManager == null || _astarManager.FinalNodeList == null || _astarManager.FinalNodeList.Count == 0)
            {
                Debug.Log("astarManager is null", this);
                return;
            }

            if (_pathIndex >= _astarManager.FinalNodeList.Count)
            {
                Debug.Log("End", this);
                return;
            }

            Vector2 targetPos = new Vector2(
                _astarManager.FinalNodeList[_pathIndex].x,
                _astarManager.FinalNodeList[_pathIndex].y
            );

            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            Vector2 curDir = Vector2.Lerp(Vector2.zero, dir, 5f * Time.deltaTime);
            transform.position += (Vector3)(curDir * (enemyDataSO.speed * Time.deltaTime));

            if (_enemyFlip != null)
            {
                int flipDir = curDir.x > 0 ? 1 : (curDir.x < 0 ? -1 : 0);
                _enemyFlip.SetFlip(flipDir);
            }

            if (Vector2.Distance(transform.position, targetPos) < 0.1f)
                _pathIndex++;
        }

        public void ResetPathIndex()
        {
            _pathIndex = 0;
        }

        public virtual void Hurt(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            isDead = true;
            Destroy(gameObject);
        }
    }
}