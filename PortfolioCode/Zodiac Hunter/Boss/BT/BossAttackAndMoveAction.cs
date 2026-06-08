using System;
using Code.Entities;
using GondrLib.PathFinder;
using Modules;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Work.PSJ.Code.Boss.Skills;
using Work.PSJ.Code.Enemies;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Boss.BT
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "BossAttackAndMove",
        story: "[Self] attack [Target] pause [MinPause] [MaxPause] minDist [MinMoveDistance]",
        category: "Boss/Action",
        id: "d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9")]
    public partial class BossAttackAndMoveAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractBoss> Self;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<float> MinPause;
        [SerializeReference] public BlackboardVariable<float> MaxPause;
        [SerializeReference] public BlackboardVariable<float> MinMoveDistance;

        private enum State { Moving, Attacking }

        private IEnemyAttackModule _attackModule;
        private BossSkillManagerCompo _skillManager;
        private BossDashPoisonTrailCompo _dashPoisonTrail;
        private PathMover _pathMover;
        private PathAgent _pathAgent;
        private State _state;
        private bool _arrived;
        private float _attackTimer;
        private float _attackDuration;

        protected override Status OnStart()
        {
            if (Self.Value == null || Target.Value == null)
                return Status.Failure;

            _attackModule = Self.Value.GetComponentInChildren<IEnemyAttackModule>();
            _skillManager = Self.Value.GetModule<BossSkillManagerCompo>();
            _dashPoisonTrail = _skillManager != null
                ? _skillManager.GetSkill<BossDashPoisonTrailCompo>()
                : Self.Value.GetComponentInChildren<BossDashPoisonTrailCompo>();
            _pathMover = Self.Value.GetModule<PathMover>();
            _pathAgent = Self.Value.GetModule<PathAgent>();

            if (_attackModule == null || _pathMover == null || _pathAgent == null)
                return Status.Failure;

            _arrived = false;
            _state = State.Moving;
            _pathMover.OnMoveEnd += OnArrived;
            MoveToRandomPosition();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Self.Value == null || Target.Value == null)
                return Status.Failure;

            switch (_state)
            {
                case State.Moving:
                    if (_arrived)
                    {
                        _arrived = false;
                        _state = State.Attacking;
                        _attackDuration = GetRandomPause();
                        _attackTimer = 0f;
                        _attackModule.SetAim(Target.Value.position);
                        _attackModule.Attack(Target.Value);
                    }
                    break;

                case State.Attacking:
                    _attackModule?.SetAim(Target.Value.position);
                    _attackTimer += Time.deltaTime;
                    if (_attackTimer >= _attackDuration)
                    {
                        _attackModule.StopAttack();
                        _state = State.Moving;
                        MoveToRandomPosition();
                    }
                    break;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (_pathMover != null)
                _pathMover.OnMoveEnd -= OnArrived;

            _dashPoisonTrail?.EndTrail();
            _attackModule?.StopAttack();
            _pathMover?.StopMove();
        }

        private void OnArrived()
        {
            _dashPoisonTrail?.EndTrail();
            _arrived = true;
        }

        private float GetRandomPause()
        {
            float min = MinPause != null && MinPause.Value > 0f ? MinPause.Value : 1f;
            float max = MaxPause != null && MaxPause.Value > min ? MaxPause.Value : min + 1f;
            return UnityEngine.Random.Range(min, max);
        }

        private void MoveToRandomPosition()
        {
            float minDist = MinMoveDistance != null && MinMoveDistance.Value > 0f ? MinMoveDistance.Value : 3f;
            Vector3 dest = _pathAgent.GetRandomNodePosition(Self.Value.transform.position, minDist);
            _dashPoisonTrail?.BeginTrail();
            _pathMover.SetDestination(dest);
        }
    }
}
