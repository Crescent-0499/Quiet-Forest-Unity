using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    [Range(0, 100)]
    public float health =100f;
    [Range(0, 100)]
    public float nutrition =100f;
    public float nutritionThreshold;
    private float nutritionTimer = 1.0f; //计时器为一秒，每一秒掉一点营养值
    private float healthTimer = 3.0f; //计时器为一秒，每一秒掉一点营养值
    public PlayerStateUI playerUI;
    private CharacterMovementController controller;

    void Start()
    {
        controller = GetComponentInParent<CharacterMovementController>();
    }
    void Update()
    {
        health = Mathf.Clamp(health, 0, 100);
        NutritionDecline();
        NutritionDamage();
        playerUI.Refresh(health,nutrition);
    }

    public void ApplyDamage(float number)
    {
        health -= number;
        if(health <0)
        {
            health = 0;
            StartCoroutine(GameOver());
        }
        if(health >100)
        {
            health = 100;
        }
    }
    // 营养值作用于生命值
    public void NutritionDamage(){
        if(nutrition <= nutritionThreshold){
            if(healthTimer > 0){
                healthTimer-=Time.deltaTime;
            }
            else{
                ApplyDamage(1.0f);
                healthTimer = 3.0f;
            }
           
        }
        if(health <0)
        {
            health = 0;
            //gameover();
        }
         if(health >100)
        {
            health = 100;
        }
    }

    //营养值跟随时间递减
    public void NutritionDecline(){
        if(nutritionTimer > 0){
            nutritionTimer-=Time.deltaTime;
        }
        else{
            nutrition--;
            nutritionTimer = 1f;
        }
    }
    IEnumerator GameOver(){
        if(controller!=null){
            controller.Death();
        } 
        yield return  new WaitForSeconds(1.5f);
        
        SceneManager.LoadScene("Start");
    }
}
