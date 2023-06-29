using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfState : MonoBehaviour
{
    [Range(0, 100)]
    public float health =100f;
    public WolfStateUI wolfUI;
    public float deltaHealth;

    void Start()
    {
        wolfUI = GetComponentInChildren<WolfStateUI>();
    }
    void Update()
    {
        health = Mathf.Clamp(health, 0, 100);
        wolfUI.Refresh(health);
        // Debug.Log("deltaHealth:"+deltaHealth);
    }

    public void ApplyDamage(float number)
    {
        deltaHealth = number; 
        health -= number;
        if(health <0)
        {
            health = 0;
        }
        if(health >100)
        {
            health = 100;
        }
    }
}
