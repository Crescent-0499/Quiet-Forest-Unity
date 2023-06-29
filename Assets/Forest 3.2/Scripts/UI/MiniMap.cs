using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Camera minimapCamera;
    public CharacterController player;
    private Vector3 offsetPosition;
    // Update is called once per frame
     void Start()
    {
        offsetPosition = transform.position - player.transform.position;
    }
    void Update()
    {
        minimapCamera.transform.position =  offsetPosition + player.transform.position;
    }
}