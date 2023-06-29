using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FootstepSound : MonoBehaviour
{
    [SerializeField] private AudioSource[] Concrete;
    [SerializeField] private AudioSource[] Dirt;
    [SerializeField] private AudioSource[] Grass;
    [SerializeField] private AudioSource[] Water;
    private string Ground;
    bool IsWalkingWait;
    bool IsRunningWait;
    bool IsCrouchingWait;
    int WalkedTransition;
    int RunTransition;
    string WalkOrRun;
    private void Start()
    {
        Ground = "Grass";
    }
    private void Update()
    {
        if(Ground == "") { Ground = "Grass"; }
        if(GetComponentInParent<CharacterMovementController>().IsWalking == true)
        {
            StartWalkingFootsteps();
        }
        if (GetComponentInParent<CharacterMovementController>().IsRunning== true)
        {
            StartRunningFootsteps();
        }
        if (GetComponentInParent<CharacterMovementController>().IsCrouching == true)
        {
            StartCrouchingFootsteps();
        }
        Debug.Log("Ground is " + Ground);
    }
    private void StartWalkingFootsteps()
    {
        if (IsWalkingWait) 
        { 
            return; 
        }
        StartCoroutine(HitFootstepWalk());
    }
    private void StartCrouchingFootsteps()
    {
        if (IsCrouchingWait) { return; }
        StartCoroutine(HitFootstepCrouch());
    }
    private void StartRunningFootsteps()
    {
        if (IsRunningWait) { return; }
        StartCoroutine(HitFootstepRun());
    }
    IEnumerator HitFootstepRun()
    {
        IsRunningWait = true;
        if (Ground == "Concrete")
        {
            if (RunTransition == 0)
            {
                RunTransition = 1;
                Concrete[2].Play();
            }
            else if (RunTransition == 1)
            {
                RunTransition = 0;
                Concrete[3].Play();
            }
        }
        if (Ground == "Dirt")
        {
            if (RunTransition == 0)
            {
                RunTransition = 1;
                Dirt[2].Play();
            }
            else if (RunTransition == 1)
            {
                RunTransition = 0;
                Dirt[3].Play();
            }
        }
        if (Ground == "Grass")
        {
            if (RunTransition == 0)
            {
                RunTransition = 1;
                Grass[2].Play();
            }
            else if (RunTransition == 1)
            {
                RunTransition = 0;
                Grass[3].Play();
            }
        }
        if (Ground == "Water")
        {
            if (RunTransition == 0)
            {
                RunTransition = 1;
                Water[2].Play();
            }
            else if (RunTransition == 1)
            {
                RunTransition = 0;
                Water[3].Play();
            }
        }
        yield return new WaitForSeconds(0.3f);
        IsRunningWait = false;
    }
    IEnumerator HitFootstepCrouch()
    {
        IsCrouchingWait = true;
        if (Ground == "Concrete")
        {
            if (WalkedTransition == 0)
            {
                WalkedTransition = 1;
                Concrete[0].Play();
            }
            else if (WalkedTransition == 1)
            {
                WalkedTransition = 0;
                Concrete[1].Play();
            }
        }
        if (Ground == "Dirt")
        {
            if (WalkedTransition == 0)
            {
                WalkedTransition = 1;
                Dirt[0].Play();
            }
            else if (WalkedTransition == 1)
            {
                WalkedTransition = 0;
                Dirt[1].Play();
            }
        }
        if (Ground == "Grass")
        {
            if (WalkedTransition == 0)
            {
                WalkedTransition = 1;
                Grass[0].Play();
            }
            else if (WalkedTransition == 1)
            {
                WalkedTransition = 0;
                Grass[1].Play();
            }
        }

        if (Ground == "Water")
        {
            if (WalkedTransition == 0)
            {
                WalkedTransition = 1;
                Water[0].Play();
            }
            else if (WalkedTransition == 1)
            {
                WalkedTransition = 0;
                Water[1].Play();
            }
        }
        yield return new WaitForSeconds(0.58f);
        IsCrouchingWait = false;
    }
    IEnumerator HitFootstepWalk()
    {
        IsWalkingWait = true;
        if(Ground == "Concrete")
        {
            if(WalkedTransition == 0)
            {
                WalkedTransition = 1;
                Concrete[0].Play();
            }
            else if(WalkedTransition == 1)
            {
                WalkedTransition = 0;
                Concrete[1].Play();
            }
        }
        if(Ground == "Dirt")
        {
            if (WalkedTransition == 0)
            {
                WalkedTransition = 1;
                Dirt[0].Play();
            }
            else if (WalkedTransition == 1)
            {
                WalkedTransition = 0;
                Dirt[1].Play();
            }
        }
        if(Ground == "Grass")
        {
            if (WalkedTransition == 0)
            {
                WalkedTransition = 1;
                Grass[0].Play();
            }
            else if (WalkedTransition == 1)
            {
                WalkedTransition = 0;
                Grass[1].Play();
            }
        }
        if (Ground == "Water")
        {
            if(WalkedTransition == 0)
            {
                WalkedTransition = 1;
                Water[0].Play();
            }
            else if (WalkedTransition == 1)
            {
                WalkedTransition = 0;
                Water[1].Play();
            }
        }
        yield return new WaitForSeconds(0.55f);
        IsWalkingWait = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 17) { Ground = other.tag; }
        Debug.Log("Ground Type is "+ Ground);
    } 
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 17) { Ground = other.tag; }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 17) { Ground = ""; }
    }
}
