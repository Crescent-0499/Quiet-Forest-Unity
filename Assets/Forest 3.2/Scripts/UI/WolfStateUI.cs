using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WolfStateUI : MonoBehaviour
{
    public Camera camera;
    public Slider healthBar;
    void Start()
    {
        camera = GameObject.Find("MainCamera").GetComponent<Camera>();
        healthBar.value = 100f;
    }

    void Update(){
        healthBar.transform.LookAt(camera.transform);
    }


    // Update is called once per frame
    public void Refresh(float health)
    {
        healthBar.value = health;
    }
}
