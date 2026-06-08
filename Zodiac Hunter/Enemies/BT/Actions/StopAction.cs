using System;
using Modules;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Work.PSJ.Code.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "StopAction", story: "Set [PathMover] [isStop] to", category: "Action", id: "d7c9eb46a7f8edb41829a83f7d68328f")]
    public partial class StopAction : Action
    {
        [SerializeReference] public BlackboardVariable<PathMover> PathMover;
        [SerializeReference] public BlackboardVariable<bool> IsStop;

        protected override Status OnStart()
        {
            if (PathMover == null || PathMover.Value == null)
            {
                Debug.LogError("PathMover is null");
                return Status.Failure;
            }

            PathMover.Value.StopMove();

            return Status.Success;
        }
    }
}

