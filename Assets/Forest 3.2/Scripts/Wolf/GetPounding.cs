using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("Wolf")]
    public class GetPounding : Conditional
    {
        private WolfState wolfState;
        public SharedFloat poundHealth;
        public SharedFloat deltaHealth;
        public SharedBool pound;
 
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
            deltaHealth.Value = wolfState.deltaHealth;
            if(deltaHealth.Value >= poundHealth.Value){
                pound.Value = true;
            }
            else{
                pound.Value = false;
            }
            
            if ( pound.Value == true) {
                // Return success if an object was found
                return TaskStatus.Success;
            }
            // An object is not within sight so return failure
            return TaskStatus.Failure;
        }

        public override void OnReset()
        {
            wolfState = null;
            deltaHealth.Value = 0f;
            pound.Value = false;
        }
    }
}