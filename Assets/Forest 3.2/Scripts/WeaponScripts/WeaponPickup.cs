using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private ActiveWeapon activeWeapon;
    private PlayerControls Controls;
    GunController weapon;
    private bool allowPickUp= false;
    private bool inCollider = false;
    public GunController weaponFab;  //武器预制体

    void Start()
    {
        weapon = GetComponent<GunController>();
        Controls = new PlayerControls();
        Controls.Enable();
        Controls.Keyboard.Equip.performed += ctx =>
        {
            allowPickUp = true;
        };
        Controls.Keyboard.Equip.canceled += ctx =>
        {
            allowPickUp = false;
        };
    }
    private void Update()
    {
        // Debug.Log( "inCollider:"+ inCollider+"allowPickUp:"+ allowPickUp+ "activeWeapon:"+ activeWeapon);
        if(inCollider && allowPickUp && activeWeapon) 
        {
            GunController newWeapon = Instantiate(weaponFab);// 创建武器对象
            if(newWeapon.isOnGround == true){
                newWeapon.isOnGround = false;
                //装备武器时根据武器有没有在地上，来启用或者禁用相关组件
                newWeapon.DisableOrEnableComponents(newWeapon.isOnGround);
                newWeapon.transform.position = Vector3.zero;
                newWeapon.transform.rotation = Quaternion.identity;
                activeWeapon.Equip(newWeapon, true);
                //销毁地上的武器
                Destroy(gameObject);
            }  
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
        if(activeWeapon && weapon.isOnGround == true) 
        {
            // Debug.Log("进入碰撞范围");
            inCollider= true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(activeWeapon && weapon.isOnGround == true) 
        {
            // Debug.Log("离开碰撞范围");
            inCollider= false;
        }
    }
    
}
