using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfAttack : MonoBehaviour
{
    private PlayerState playerState;
    public float damage_0 = 5;   //挥爪的伤害
    public float damage_1 = 10;    //跳跃攻击的伤害
    private float damage;  //应用的伤害
    public SphereCollider sphereCollider;
    public Animator wolfAnimator;
    void Start(){
        sphereCollider = GetComponent<SphereCollider>(); 
        wolfAnimator = GetComponentInParent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        playerState = other.GetComponent<PlayerState>();
        if(playerState != null  && wolfAnimator != null ){
            int attackType = wolfAnimator.GetInteger("attackType");
            if(attackType == 0)
                damage = damage_0;
            if(attackType == 1)
                damage = damage_1;
            playerState.ApplyDamage(damage);
        }
        
    }
    private void OnTriggerExit(Collider other)
    {

    }
}
