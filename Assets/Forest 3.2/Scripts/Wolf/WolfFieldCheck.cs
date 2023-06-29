using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfFieldCheck : MonoBehaviour
{
    public GameObject otherGameObject;
    public string targetTag;
    public bool enteredTrigger;
    void Start()
    {
        enteredTrigger = false;
    }

     private void OnTriggerEnter(Collider other)
    {
        Debug.Log("you are in wolf Field");
         if (string.IsNullOrEmpty(targetTag) || other.gameObject.CompareTag(targetTag)) {
                otherGameObject = other.gameObject;
                enteredTrigger = true;
            }
    }
    private void OnTriggerExit(Collider other)
    {
        
    }
}
