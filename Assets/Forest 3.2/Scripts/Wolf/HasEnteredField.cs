using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("Wolf")]
    public class HasEnteredField : Conditional
    {
        [Tooltip("The object that is within Field")]
        public SharedGameObject returnedObject;

        private WolfFieldCheck wolfField;

        public override void OnStart()
        {
           GameObject gameObject = GameObject.FindWithTag("WolfField");
           wolfField = gameObject.GetComponent<WolfFieldCheck>();
        }

        public override TaskStatus OnUpdate()
        {
           if( wolfField.enteredTrigger == true){
                Debug.Log("I can see you!");
                returnedObject.Value =  wolfField.otherGameObject ;
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
            returnedObject.Value = null;
        }
    }
}