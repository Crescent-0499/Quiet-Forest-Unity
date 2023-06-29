using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Unity.Wolf
{
    [TaskCategory("Wolf")]
    public class IsHealthLow : Conditional
    {
        public SharedFloat health;
        private WolfState wolfState;
        
        public override void OnStart()
        {
           wolfState= GetComponent<WolfState>();
        }

        public override TaskStatus OnUpdate()
        {
            if (wolfState == null) {
                Debug.LogWarning("wolfState is null");
                return TaskStatus.Failure;
            }
            // Debug.Log(health.Value);
            health.Value = wolfState.health;
            if(health.Value <= 10 && health.Value>0)
                return TaskStatus.Success;
            else return TaskStatus.Failure;
        }

        public override void OnReset()
        {
            wolfState = null;
            health = 0f ;
        }
    }
}
