using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(SphereCollider))]
public class GunController : MonoBehaviour
{
    [Header("Shooting Attribute")]
    private CrossHairTarget target;    //射击目标
    [SerializeField] private Transform RaycastOrigin;
    [SerializeField] private Transform RaycastDestination;
    Ray ray;
    RaycastHit hitInfo;
    [SerializeField] private GameObject hitArea;
    LayerMask layerMask;
    
    [Header("Weapons Attribute")]
    public int fireRate = 25;   //射速
    public ActiveWeapon.WeaponSlot WeaponSlotType; //武器类型
    public int ammoCount;
    public int ammoGross;
    public int ClipSize;
    public float Damage = 10f; // 伤害
    public bool AxeAttack;
    float accumalatedTime;
    
    public WeaponProceduralRecoil recoil { get; set; }
    public bool isFiring;
    public GameObject Magazine;
    public bool isOnGround;
    Collider colBox;
    public Collider colSphere;
    Rigidbody rgbody;
    public GunController weaponFab;  //武器预制体   
    [Range(0f, 1f)]  //随机数
    public float ricochetChance; //弹开？
    public float minRicochetAngle; //弹开角度

    [Header("Shooting Effect")]
    [SerializeField] private ParticleSystem[] muzzleFlash;
    [SerializeField] private ParticleSystem hiteffect;
    private TrailRenderer tracerEffect;
    [SerializeField] private AudioClip fireAudioClip;
    public AudioClip reloadAudioClip;
    public AudioSource audioSource;

    private void Awake()
    {
        recoil = GetComponent<WeaponProceduralRecoil>();
    }
    private void Start()
    {
        layerMask = LayerMask.GetMask("FieldSphere");
        hitArea = GameObject.FindGameObjectWithTag("HitArea");
        target = FindObjectOfType<CrossHairTarget>();
        // Debug.Log(target);
        RaycastDestination = target.gameObject.transform;
        // 轨道特效组件
        tracerEffect = FindObjectOfType<ActiveWeapon>().renderer;
        audioSource = GetComponent<AudioSource>();
    }
    public void StartAxeAttack(Animator Main, Animator Rig)
    {
        // 进入斧头攻击状态
        AxeAttack = true;
        Main.SetBool("AttackAxe", true);
        Rig.Play("Weapon_Constrain_Axe", 1);
        StartCoroutine(AxeStopSequence(Main, Rig));
        Invoke("CalculateAxeHit", 0.75f);
    }
    IEnumerator AxeStopSequence(Animator Main, Animator Rig)
    {
        yield return new WaitForSeconds(((2.267f/1.25f)/Main.GetCurrentAnimatorStateInfo(0).speed - 0.3f) - 0.25f);
        Main.SetBool("AttackAxe", false);
        Rig.SetBool("ConstrainAxe", false);
        yield return new WaitForSeconds(0.25f);
        // 斧头攻击状态结束
        AxeAttack = false;
    }

     private void CalculateAxeHit()
    {
        // 返回一个数组，其中包含与球体接触或位于球体内部的所有碰撞体
        Collider[] hitcolliders = Physics.OverlapSphere(transform.position, 3f);
        foreach(var hitCollider in hitcolliders)
        {
            if(hitCollider.tag == "Tree")
            {
                hitCollider.gameObject.GetComponent<TreeDamageable>().Break();
            }
             if (hitCollider.tag == "NPC")
            {
                hitCollider.gameObject.GetComponent<WolfState>().ApplyDamage(Damage);
            }
        }
    }

    public void StartFiring()
    {
        isFiring = true;  //进入开火状态
        recoil.Reset();
        accumalatedTime = 0.0f;
    }
    public void UpdateFiring(float deltaTime)
    {
        accumalatedTime += deltaTime;
        float fireInterval = 1.0f / fireRate;
        while(accumalatedTime >= 0.0f)
        {
            FireBullet();
            accumalatedTime -= fireInterval;
        }
    }
    private void FireBullet()
    {
        if(ammoCount <= 0)
        {
            return;
        }  
        ammoCount--;
        recoil.GenerateRecoil(WeaponSlotType.ToString());
        muzzleFlash[0].Emit(1);
        muzzleFlash[1].Emit(1);
        // 发射源，目标点，渲染轨迹
        ray.origin = RaycastOrigin.position;
        ray.direction = RaycastDestination.position - RaycastOrigin.position;
        var tracer = Instantiate(tracerEffect, ray.origin, Quaternion.identity);
        tracer.AddPosition(ray.origin);
        audioSource.PlayOneShot(fireAudioClip, 0.3F);
        if (Physics.Raycast(ray, out hitInfo, 1000f,~layerMask) && hitInfo.collider.tag != "Player")
        {
            // Debug.DrawLine( ray.origin, hitInfo.transform.position , Color.yellow , 1f);
            Debug.Log("Guncontroller:"+hitInfo.collider.gameObject.name);
            hiteffect.transform.position = hitInfo.point;
            hiteffect.transform.forward = hitInfo.normal;
            hiteffect.Emit(1);
            tracer.transform.position = hitInfo.point;
            var rb2d = hitInfo.collider.GetComponent<Rigidbody>();
            if (hitInfo.collider.gameObject.GetComponent<Destructable>() != null)
            {
                hitInfo.collider.gameObject.GetComponent<Destructable>().DestroyAndReplace();
            }
            if (rb2d && hitInfo.collider.gameObject.tag != "Water" && hitInfo.collider.gameObject.tag != "Dirt")
            {
                rb2d.AddForceAtPosition(ray.direction * 20, hitInfo.point, ForceMode.Impulse);
            }
            if (hitInfo.collider.gameObject.tag == "NPC")
            {
                hitInfo.collider.gameObject.GetComponent<WolfState>().ApplyDamage(Damage);
            }
        }

    }
    public void StopFiring()
    {
        isFiring = false;
    }
    
    public void DisableOrEnableComponents(bool enabled)
    {
        colBox = GetComponent<BoxCollider>();
        rgbody = GetComponent<Rigidbody>();
        colSphere = GetComponent<SphereCollider>();
        if(!enabled)
        {
            // 如果启用了 isKinematic，则力、碰撞或关节将不再影响刚体。
            rgbody.isKinematic = true;
            colBox.enabled = false;
            colSphere.enabled = false;
        }
        if(enabled)
        {   //在地上，启用 colBox、rgbody、colSphere
            rgbody.isKinematic = false;
            colBox.enabled = true;
            colSphere.enabled = true;
        }
    }

    private void AddOrDecAmmoGross(){

    }
}
