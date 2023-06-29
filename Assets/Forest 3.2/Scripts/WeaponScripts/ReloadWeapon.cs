using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadWeapon : MonoBehaviour
{
    [SerializeField] private Animator rigController;
    public AudioClip reloadAudioClip;
    AudioSource audioSource;
    private bool isReloading;
    private PlayerControls Controls;
    public CharacterController playerController;
    public WeaponAnimationEvents animationEvents;
    private ActiveWeapon activeWeapon;
    public Transform lefthand;
    public WeaponState weaponState;  //武器状态UI
    GameObject magazineHand;
    private void Start()
    {
        // 获取玩家碰撞体
        playerController = GetComponentInParent<CharacterController>();
        activeWeapon = GetComponent<ActiveWeapon>();
        // 向 UnityEvent 添加非持久性监听器，OnAnimationEvent为执行的动作
        animationEvents.WeaponAnimationEvent.AddListener(OnAnimationEvent);
        isReloading= false;
        // 输入系统开启
        Controls = new PlayerControls();
        Controls.Enable();
        Controls.Keyboard.Reload.performed += ctx =>
        {
            GunController weapon = activeWeapon.GetActiveWeapon();
            // 武器不空，弹夹没满，武器不是斧头
            if (weapon && weapon.ammoCount != weapon.ClipSize && weapon.WeaponSlotType.ToString() != "Axe"){
                audioSource = weapon.audioSource;
                reloadAudioClip = weapon.reloadAudioClip;
                audioSource.PlayOneShot(reloadAudioClip, 0.5F);
                // 播放换弹动画
                rigController.SetTrigger("reload_weapon");
            }

        };
    }
    private void Update()
    {
        GunController weapon = activeWeapon.GetActiveWeapon();
        // 武器不空，弹夹空了，武器不是斧头，自动换子弹
        if (weapon && weapon.ammoCount <= 0 && weapon.WeaponSlotType.ToString() != "Axe" && !isReloading)
        {
            isReloading=true;
            StartCoroutine(ReloadAudio(weapon));
            rigController.SetTrigger("reload_weapon");
        }
        if (weapon)
        {
            // 更新弹药显示信息
            weaponState.Refresh(weapon.ammoCount, weapon.ClipSize, weapon.WeaponSlotType.ToString());
        }
    }
    void OnAnimationEvent(string eventName)
    {
        switch (eventName)
        {
            // 分离弹匣     
            case "detach_magazine":
                DetachMagazine();
                break;
            // 丢掉弹匣
            case "drop_magazine":
                DropMagazine();
                break;
            // 填满弹匣
            case "refill_magazine":
                RefillMagazine();
                break;
            // 附加弹匣
            case "attach_magazine":
                AttachMagazine();
                break;
        }
    }

    private void DetachMagazine()
    {
        GunController weapon = activeWeapon.GetActiveWeapon();
        lefthand = weapon.transform;
        // 指定父对象时，传递true可直接在世界空间中定位新对象。
        // lefthand为父对象,位置为武器的位置，这样新产生的弹匣可以跟随武器
        magazineHand = Instantiate(weapon.Magazine, lefthand, true);
        weapon.Magazine.SetActive(false);
    }

     private void DropMagazine()
    {   
        // 复制抛弃的弹夹对象
        GameObject droppedMagazine = Instantiate(magazineHand, magazineHand.transform.position, magazineHand.transform.rotation);
        // 添加刚体组件
        droppedMagazine.AddComponent<Rigidbody>();
        droppedMagazine.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        droppedMagazine.AddComponent<BoxCollider>();
        // 忽略玩家和弹匣之间的碰撞
        Physics.IgnoreCollision(droppedMagazine.GetComponent<BoxCollider>(),playerController,true);
        magazineHand.SetActive(false);
        // 启动协程，在几秒后摧毁被抛弃的弹夹
        StartCoroutine(DestroyClip(droppedMagazine));
    }

     private void RefillMagazine()
    {
        magazineHand.SetActive(true);
    }

    private void AttachMagazine()
    {
        GunController weapon = activeWeapon.GetActiveWeapon();
        Destroy(magazineHand);
        weapon.Magazine.SetActive(true);
        weapon.ammoCount = weapon.ClipSize;
        rigController.ResetTrigger("reload_weapon");
        weaponState.Refresh(weapon.ammoCount, weapon.ClipSize, weapon.WeaponSlotType.ToString());
    }

    IEnumerator DestroyClip(GameObject clip)
    {
        yield return new WaitForSeconds(7f);
        Destroy(clip.gameObject);
    }

    IEnumerator ReloadAudio(GunController weapon)
    {
        reloadAudioClip = weapon.reloadAudioClip;
        weapon.audioSource.PlayOneShot(reloadAudioClip, 0.5F);
        yield return new WaitForSeconds(2f);
        isReloading = false;
    }
}
