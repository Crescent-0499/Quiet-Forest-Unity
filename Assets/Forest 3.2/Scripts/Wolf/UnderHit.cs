using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("Wolf")]
    public class UnderHit : Conditional
    {
        private WolfState wolfState;
        public SharedFloat deltaHealth;

        [Tooltip("The object that is within sight")]
        public SharedGameObject returnedObject;

         [Tooltip("The tag of the object that we are searching for")]
        public SharedString targetTag;
        
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
            deltaHealth.Value = wolfState.deltaHealth;
            if(deltaHealth.Value>0){
                if(targetTag.Value != null)
                    returnedObject.Value = GameObject.FindGameObjectWithTag(targetTag.Value);
                else Debug.Log("targetTag.Value is null");
            }
            
            if (returnedObject.Value != null) {
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
            returnedObject = null;
        }
    }
}