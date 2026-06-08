using System;
using Code.Entities;
using GondrLib.PathFinder;
using Modules;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Work.PSJ.Code.Boss.Skills;
using Work.PSJ.Code.Enemies.BT;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Boss.BT
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossReposition", story: "[Self] reposition to random position", category: "Boss/Action", id: "c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8")]
    public partial class BossRepositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractBoss> Self;

        private PathMover _pathMover;
        private PathAgent _pathAgent;
        private BossSkillManagerCompo _skillManager;
        private BossDashPoisonTrailCompo _dashPoisonTrail;
        private bool _arrived;

        protected override Status OnStart()
        {
            if (Self.Value == null)
                return Status.Failure;

            _pathMover = Self.Value.GetModule<PathMover>();
            if (_pathMover == null)
                return Status.Failure;

            _pathAgent = Self.Value.GetModule<PathAgent>();
            _skillManager = Self.Value.GetModule<BossSkillManagerCompo>();
            _dashPoisonTrail = _skillManager != null
                ? _skillManager.GetSkill<BossDashPoisonTrailCompo>()
                : Self.Value.GetComponentInChildren<BossDashPoisonTrailCompo>();
            _arrived = false;

            _pathMover.OnMoveEnd += OnArrived;
            MoveToRandomPosition();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (_arrived)
            {
                Self.Value.SetVariableValue(BTVariables.CurrentState, Self.Value.CurrentPhase);
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (_pathMover != null)
            {
                _pathMover.OnMoveEnd -= OnArrived;
                _dashPoisonTrail?.EndTrail();
                _pathMover.StopMove();
            }
        }

        private void OnArrived()
        {
            _dashPoisonTrail?.EndTrail();
            _arrived = true;
        }

        private void MoveToRandomPosition()
        {
            if (_pathAgent == null) return;
            _dashPoisonTrail?.BeginTrail();
            _pathMover.SetDestination(_pathAgent.GetRandomNodePosition(Self.Value.transform.position, 3f));
        }
    }
}
