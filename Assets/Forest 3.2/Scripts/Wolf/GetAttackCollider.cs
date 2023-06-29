using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Tasks.Unity
{
    [TaskCategory("Wolf")]
    [TaskDescription("Enables/Disables the collider. Returns Success.")]
    public class GetAttackCollider : Action
    {
        public SharedCollider returnCollider;
        private SphereCollider sphereCollider;

        public override void OnStart()
        {
           sphereCollider = gameObject.GetComponentInChildren<SphereCollider>(); 
        }

        public override TaskStatus OnUpdate()
        {
            if (sphereCollider == null) {
                return TaskStatus.Failure;
            }
            returnCollider.Value = sphereCollider;
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            returnCollider.Value = null;
            sphereCollider = null;
        }
    }
}