using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    public enum WeaponSlot
    {
        SMG = 0,
        Pistol = 1,
        Axe = 2,
        Rifle = 3,
        Shotgun = 4,
        Sniper = 5
    }

    [Header("Customizables")]
    public bool HideInventoryWeapons = false;  //隐藏库存武器
    CharacterController playerController;
    CharacterMovementController characterMovementController;

    [Header("Variables")]
    GunController[] Equipped_Weapons = new GunController[6];  //已装备的武器数组
    GunController currentWeapon; //当前手中的武器对象
    public int activeWeaponIndex;  //当前活动武器序号
    public int weaponseq=0;
    bool HolsteredWeapon;   //收起/装备
    bool WasHolstered;  //有备用武器吗
    bool RemoveWeaponCurrent;
    bool isDropping;  //正在丢武器
    public float dropForce;   //丢弃武器的力量
    float punchcombo;
    public Transform[] weaponSlots;
    public Transform WeaponLeftGrip;
    public Transform WeaponRightGrip;
    public Animator rigController;
    private Animator TPSLocomotion;
    private PlayerControls Controls;
    public TrailRenderer renderer;
    public WeaponState weaponState;  //弹药显示界面
    WeaponAiming aiming;
    [SerializeField] private Cinemachine.CinemachineFreeLook playerCamera;
    public bool CancelAllMovement { get; set; }

    void Start()
    {
        punchcombo = 0;
        aiming = GetComponent<WeaponAiming>();
        activeWeaponIndex = 0;
        TPSLocomotion = GetComponent<Animator>();
        HolsteredWeapon = false;
        isDropping = false;
        playerController = GetComponent<CharacterController>();
        characterMovementController = GetComponent<CharacterMovementController>();
        Controls = new PlayerControls();
        Controls.Enable();
        //收起武器、拿出武器
        Controls.Keyboard.HolsterEquip.performed += ctx =>
        {
            var controller = FindObjectOfType<UIController>();
            if (controller.CancelAllMovement == true) { return; }
                // isHolstered是状态机中 Holster_Weapon 的值
                bool isHolstered = rigController.GetBool("Holster_Weapon");
                HolsteredWeapon = !isHolstered;
                // 根据HolsteredWeapon决定播放收枪还是拿出枪的动画
                rigController.SetBool("Holster_Weapon", HolsteredWeapon);
        };
        // 切换武器
        Controls.Keyboard.MoveThrough.performed += ctx =>
            {
                var controller = FindObjectOfType<UIController>();
                if (controller.CancelAllMovement == true) { return; }
                int x = 0;
                for (int i = activeWeaponIndex; i < Equipped_Weapons.Length + 1; i++)
                {
                    x++;
                    if (i == Equipped_Weapons.Length) { i = 0; }
                    if (Equipped_Weapons[i] != null && i != activeWeaponIndex)
                    {
                        SetActiveWeapon((WeaponSlot)i);
                        return;
                    }
                    if (x == Equipped_Weapons.Length) { return; }
                }
            };
            // 切换回刚才的武器
        Controls.Keyboard.MoveBack.performed += ctx =>
            {
                var controller = FindObjectOfType<UIController>();
                if (controller.CancelAllMovement == true) { return; }
                int x = 0;
                for (int i = activeWeaponIndex; i > -2; i--)
                {
                    x++;
                    if (i == -1) { i = Equipped_Weapons.Length - 1; }
                    if (Equipped_Weapons[i] != null && i != activeWeaponIndex)
                    {
                        SetActiveWeapon((WeaponSlot)i);
                        return;
                    }
                    if (x == Equipped_Weapons.Length) { return; }
                }
            };
        Controls.Keyboard.RemoveWeapon.performed += ctx =>
            {
                var controller = FindObjectOfType<UIController>();
                if (controller.CancelAllMovement == true) { return; }
                RemoveWeaponCurrent = true;
            };
        Controls.Keyboard.Shoot.started += ctx =>
         {
                var controller = FindObjectOfType<UIController>();
                if (controller.CancelAllMovement == true) { return; }
                characterMovementController.IsRunning = false;
                characterMovementController.IsCrouching = false;
                characterMovementController.IsWalking = false;
                if (currentWeapon != null && !HolsteredWeapon)
                {
                    //当前武器类型是斧头
                    if (currentWeapon.WeaponSlotType.ToString() == "Axe")
                    {
                        if(GetComponent<CharacterMovementController>().Crouched == true) { return; }
                        if (currentWeapon.AxeAttack == true) return;
                        // 斧头攻击
                        currentWeapon.StartAxeAttack(TPSLocomotion, rigController);
                    }
                        // 不是斧头，可以开火
                        currentWeapon.StartFiring();
                }
                // 当前武器不空但是背着武器||当前武器为空，进行拳击
                else if (currentWeapon != null && HolsteredWeapon || currentWeapon == null)
                {
                    // 打拳的
                    if (punchcombo == 0)
                    {
                        punchcombo = 1;
                        StartPunchAttack(punchcombo);
                    }
                    else if (punchcombo == 1)
                    {
                        punchcombo = 2;
                        StartPunchAttack(punchcombo);
                    }
                    else if (punchcombo == 2)
                    {
                        punchcombo = 1;
                        StartPunchAttack(punchcombo);
                    }
                }
            };
        Controls.Keyboard.Shoot.canceled += ctx =>
            {
                var controller = FindObjectOfType<UIController>();
                if (controller.CancelAllMovement == true) { return; }
                if (FindObjectOfType<UIController>().CancelAllMovement == true) { return; }
                if (HolsteredWeapon) { return; }
                if (currentWeapon != null)
                {
                    if (currentWeapon.WeaponSlotType.ToString() == "Axe") { return; }
                    // 停止射击
                    currentWeapon.StopFiring();
                }
            };
        rigController.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        rigController.updateMode = AnimatorUpdateMode.Normal;
        //获取子组件中的武器组件
        GunController exsistingWeapon = GetComponentInChildren<GunController>();
        //如果已存在这个武器就销毁状态下装备
        if (exsistingWeapon)
            Equip(exsistingWeapon, true);
    }
    void SetHolstered()
    {
        bool isHolstered = rigController.GetBool("Holster_Weapon");
        HolsteredWeapon = !isHolstered;
        rigController.SetBool("Holster_Weapon", HolsteredWeapon);
    }
    private void Update()
    {
        if (TPSLocomotion.GetCurrentAnimatorStateInfo(0).IsName("PunchRight") && TPSLocomotion.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f || TPSLocomotion.GetCurrentAnimatorStateInfo(0).IsName("PunchLeft") && TPSLocomotion.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) 
        { CancelAllMovement = true; } 
        else
        {
            CancelAllMovement = false;
        }
        if (currentWeapon != null)
        {
            aiming.currentWeapon = currentWeapon.WeaponSlotType.ToString();
        }
        else
        {
            aiming.currentWeapon = "";
        }
        if (currentWeapon == null) { return; } 
        else if (currentWeapon.AxeAttack) 
            { CancelAllMovement = true; } 
        else { CancelAllMovement = false; }
        // 更新当前武器
        var weapon = GetWeapon(activeWeaponIndex);
        currentWeapon = weapon;
        // 是不是在扔枪
        RemoveWeapon();
        // 没枪不执行
        if (currentWeapon == null) return;
        // 在开火的话，执行开火
        if (currentWeapon.isFiring)
        {
            weapon.UpdateFiring(Time.deltaTime);
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        SetRigLayerWeight();
    }
    private void SetRigLayerWeight()
    {
        if (currentWeapon == null)
        {
            TPSLocomotion.SetLayerWeight(0, 1f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 0f);
            rigController.Play("Weapon_Unarmed");
            activeWeaponIndex = 0;
            return;
        }
        if (HolsteredWeapon && activeWeaponIndex == 0)
        {
            TPSLocomotion.SetLayerWeight(0, 1f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        if (HolsteredWeapon && activeWeaponIndex == 1)
        { 
            TPSLocomotion.SetLayerWeight(0, 1f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        if (HolsteredWeapon && activeWeaponIndex == 2)
        {
            TPSLocomotion.SetLayerWeight(0, 1f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        if (HolsteredWeapon && activeWeaponIndex == 3)
        {
            TPSLocomotion.SetLayerWeight(0, 1f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        if (HolsteredWeapon && activeWeaponIndex == 4)
        {
            TPSLocomotion.SetLayerWeight(0, 1f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        if (HolsteredWeapon && activeWeaponIndex == 5)
        {
            TPSLocomotion.SetLayerWeight(0, 1f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        else if (!HolsteredWeapon && activeWeaponIndex == 2)
        {
            TPSLocomotion.SetLayerWeight(0, 0f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 1f);
        }
        else if (!HolsteredWeapon && activeWeaponIndex == 1)
        {
            TPSLocomotion.SetLayerWeight(0, 0f);
            TPSLocomotion.SetLayerWeight(1, 1f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        else if (!HolsteredWeapon && activeWeaponIndex == 0)
        {
            TPSLocomotion.SetLayerWeight(0, 0f);
            TPSLocomotion.SetLayerWeight(1, 1f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        else if (!HolsteredWeapon && activeWeaponIndex == 3)
        {
            TPSLocomotion.SetLayerWeight(0, 0f);
            TPSLocomotion.SetLayerWeight(1, 1f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        else if (!HolsteredWeapon && activeWeaponIndex == 4)
        {
            TPSLocomotion.SetLayerWeight(0, 0f);
            TPSLocomotion.SetLayerWeight(1, 1f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        else if (!HolsteredWeapon && activeWeaponIndex == 5)
        {
            TPSLocomotion.SetLayerWeight(0, 0f);
            TPSLocomotion.SetLayerWeight(1, 1f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
        else
        {
            TPSLocomotion.SetLayerWeight(0, 1f);
            TPSLocomotion.SetLayerWeight(1, 0f);
            TPSLocomotion.SetLayerWeight(2, 0f);
        }
    }
    
    // 装备武器
    public void Equip(GunController newWeapon, bool destroy)
    {
        var controller = FindObjectOfType<UIController>();
        if (controller.CancelAllMovement == true) { return; }
        rigController.ResetTrigger("reload_weapon");
        WasHolstered = HolsteredWeapon;
        // HolsteredWeapon如果再执行，接下来的动作目标值
        SetHolstered();
        int weaponSlotIndex = (int)newWeapon.WeaponSlotType;
        var weapon = GetWeapon(weaponSlotIndex);
        if (destroy)
        {
            if (weapon)
            {
               Destroy(weapon.gameObject);
            }
        }
        weapon = newWeapon;
        currentWeapon = newWeapon;
        if(weapon.WeaponSlotType.ToString() != "Axe")
        {
            weapon.recoil.playerCamera = playerCamera;
            weapon.recoil.RigController = rigController;
        }
        //父对象不再是武器槽
        weapon.transform.SetParent(weaponSlots[weaponSlotIndex], false);
        Equipped_Weapons[weaponSlotIndex] = weapon;
        // weaponseq++;
        weaponState.Refresh(weapon.ammoCount, weapon.ClipSize, weapon.WeaponSlotType.ToString());
        SetActiveWeapon(newWeapon.WeaponSlotType);
    }
    void SetActiveWeapon(WeaponSlot weaponSlot)
    {
        // 收起武器的序号等于当前活动武器序号
        int holsterIndex = activeWeaponIndex;
        // 接收要激活武器的序号
        int activateIndex = (int)weaponSlot;
        // 收起武器的序号等于接收激活武器的序号
        if(holsterIndex == activateIndex)
        {
            holsterIndex = -1;
        }
        // 启动交换武器的协程序
        StartCoroutine(SwitchWeapon(holsterIndex, activateIndex));
    }

    private void RemoveWeapon()
    {
        if (RemoveWeaponCurrent) // 扔掉武器的按键被按下
        {
            gameObject.GetComponent<WeaponAiming>().StopAiming();// 停止射击
            weaponState.Refresh(0, 0, null);
            RemoveWeaponCurrent = false;            // 恢复默认值
            var weapon = GetWeapon(activeWeaponIndex);// var自动判断数据类型
            if(!isDropping){
                // Debug.Log("扔武器");
                isDropping = true;
                //启用 武器的刚体，boxcollider相关组件
                weapon.DisableOrEnableComponents(true);
                // 创建扔出去的武器，然后对刚体施加一个力，销毁手中的武器。
                GunController droppedWeapon = Instantiate(weapon,weaponSlots[activeWeaponIndex].position,weapon.transform.rotation);
                Rigidbody weapon_rb= droppedWeapon.GetComponent<Rigidbody>();
                // 忽略玩家和弹匣之间的碰撞
                Physics.IgnoreCollision(droppedWeapon.GetComponent<BoxCollider>(),playerController,true);
                weapon_rb.AddForce((weapon_rb.transform.forward) * dropForce);
                weapon_rb.AddTorque((weapon_rb.transform.up + weapon_rb.transform.right) * Random.Range(-3, 5));
                Destroy(weapon.gameObject);
                StartCoroutine(CanDrop(droppedWeapon));
            }
            // 在 time 秒后调用 methodName 方法。
            Invoke("EquipNewWeapon", 0.1f);
        }
    }

      // 该函数由RemoveWeapon()函数调用
    private void EquipNewWeapon()
    {
        Debug.Log("新武器");
        if (RemoveWeaponCurrent)
            return;
        var weapon = GetWeapon(activeWeaponIndex);
        if (weapon == null)
        {
            foreach (GunController controller in Equipped_Weapons)
            {
                if (controller != null)
                {
                    Equip(controller, false);
                }
            }
            Debug.Log(Equipped_Weapons);
        }
        else{
            Equip(weapon, false);
        }
        if (currentWeapon == null)
        {
            weaponState.Refresh(0, 0, null);
            Debug.Log("没有背的武器了");
            HolsteredWeapon = false;
        }
    }
    public GunController GetWeapon(int index)
    {
        if(index < 0 || index >= Equipped_Weapons.Length)
        {
            return null;
        }
        return Equipped_Weapons[index];
    }
    public GunController GetActiveWeapon()
    {
        return GetWeapon(activeWeaponIndex);
    }
    IEnumerator SwitchWeapon(int holsterindex, int activeindex)
    {
        rigController.ResetTrigger("reload_weapon");
        activeWeaponIndex = activeindex;
        yield return StartCoroutine(HolsterWeapon(holsterindex));
        yield return StartCoroutine(ActivateWeapon(activeindex));
    }
    IEnumerator HolsterWeapon(int index)
    {
        rigController.ResetTrigger("reload_weapon");
        HolsteredWeapon = true;
        var weapon = GetWeapon(index);
        rigController.SetBool("Holster_Weapon", true);
        if (weapon == null || weapon != null && WasHolstered)
        {
           yield return new WaitForSeconds(0f);
        }
        else
        {
           yield return new WaitForSeconds(0.583f + 0.25f);
        }
        if (HideInventoryWeapons)
        {
            if(weapon != null)
            weapon.gameObject.SetActive(false);
        }
    }
    //拿出武器
    IEnumerator ActivateWeapon(int index)
    {
        rigController.ResetTrigger("reload_weapon");
        SetHolstered();
        WasHolstered = HolsteredWeapon;
        var weapon = GetWeapon(index);
        if (HideInventoryWeapons)
        {
            weapon.gameObject.SetActive(true);
        }
        if (weapon)
        {
            rigController.SetBool("Holster_Weapon", false);
            rigController.Play("equip_" + weapon.WeaponSlotType);
            do
            {
                yield return new WaitForEndOfFrame();
            } while (rigController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
        }
        HolsteredWeapon = false;
    }
    void StartPunchAttack(float combo)
    {
        if(TPSLocomotion==null){return;}
        TPSLocomotion.SetFloat("PunchSide", combo);
        TPSLocomotion.SetTrigger("Punch");
    }
    IEnumerator CanDrop(GunController weapon)
    {
        yield return new WaitForSeconds(1.0f);
        weapon.isOnGround = true;
        isDropping=false;
    }
}
