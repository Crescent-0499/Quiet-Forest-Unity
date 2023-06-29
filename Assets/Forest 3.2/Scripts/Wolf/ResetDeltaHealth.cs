using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("Wolf")]
    public class ResetDeltaHealth : Action
    {
        private WolfState wolfState;
 
        public override void OnStart()
        {
           wolfState = GetComponent<WolfState>();
        }

        public override TaskStatus OnUpdate()
        {
            if (wolfState == null) {
                Debug.LogWarning("wolfState is null");
                return TaskStatus.Failure;
            }
                wolfState.deltaHealth = 1f;
                return TaskStatus.Success;
        }

        public override void OnReset()
        {
            wolfState = null;
        }
    }
}