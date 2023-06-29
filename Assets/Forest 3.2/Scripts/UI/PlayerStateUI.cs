using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : MonoBehaviour
{
    public TMPro.TMP_Text healthText;
    public TMPro.TMP_Text nutritionText;
    public Slider healthBar;
    public Slider nutritionBar;
    // Start is called before the first frame update
    void Start()
    {
        healthText.text = "100/100";
        nutritionText.text = "100/100";
        healthBar.value = 100;
        nutritionBar.value = 100;
    }

    // Update is called once per frame
    public void Refresh(float health,float nutrition)
    {
        healthBar.value = health;
        healthText.text = Mathf.Round(healthBar.value).ToString()+"/100";
        nutritionBar.value = nutrition;
        nutritionText.text = Mathf.Round(nutritionBar.value).ToString()+"/100";
    }
}
