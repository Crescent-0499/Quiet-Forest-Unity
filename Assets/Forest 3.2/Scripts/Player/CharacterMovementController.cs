using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public enum CharacterState
{
    Normal
}
public class CharacterMovementController : MonoBehaviour
{
    public CharacterState state;
    Vector3 PlayerMoveInput;
    Vector2 MouseMoveInput;
    Vector3 velocity;
    Animator animator;
    [SerializeField] private Rig aimLayer;
    private float VelocityX;
    private float VelocityZ;
    private float accleration = 3f; //加速
    private float deccleration = 4f;//减速
    private float Speed; 
    public float SpeedMultiplier;  //速度倍增器
    public float jumpSpeed;
    //CharacterController是U3D自带的碰撞检测组件，有移动，地面检测等方法可以调用
    private CharacterController Controller;
    //新输入系统生成脚本的交互类，资源文件命名为PlayerControls，类名就叫这个
    private PlayerControls Controls;
    [SerializeField] private float ConstGravity = -9.81f;
    private Vector3 MoveVec;
    //下面是状态变量
    bool running;
    bool crouching;
    bool jumping;
    bool aiming;
    public bool Crouched;
    [HideInInspector] //使变量不显示在 Inspector 中，但进行序列化。
    public bool IsWalking;
    public bool IsRunning;
    public bool IsCrouching;
    //SerializeField  强制 Unity 对私有字段进行序列化。
    //当 Unity 对脚本进行序列化时，仅对公共字段进行序列化。 
    //如果还需要 Unity 对私有字段进行序列化， 可以将 SerializeField 属性添加到这些字段。
    [SerializeField] private Camera cam;
    UIController controller;
    private ActiveWeapon weapon;
    private AmmoWidget widget;
    [SerializeField]public float turnSmoothTime = 0.08f;
    private float turnSmoothVelocity = 2f;
    [SerializeField] private GameObject CameraLookAt;
    [SerializeField] private GameObject CameraLookAtOffset;
    private Vector3 CameraLookAtOffsetVector;
    private void Start()
    {
        //界面文字显示的组件获取
        widget = transform.parent.gameObject.GetComponentInChildren<AmmoWidget>();
        state = CharacterState.Normal;
        //武器状态组件
        weapon = GetComponent<ActiveWeapon>();
        //Enabling Input Asset
        //动画控制组件
        animator = GetComponent<Animator>();
        //UI控制组件
        controller = GetComponentInChildren<UIController>();
        //获取输入动作
        Controls = new PlayerControls();
        Controls.Enable();
        Controls.Keyboard.MovementKeyBinds.performed += ctx =>
        {
            PlayerMoveInput = new Vector3(ctx.ReadValue<Vector2>().x, PlayerMoveInput.y, ctx.ReadValue<Vector2>().y);
        };
        Controls.Keyboard.MovementKeyBinds.canceled += ctx =>
        {
            PlayerMoveInput = new Vector3(ctx.ReadValue<Vector2>().x, PlayerMoveInput.y, ctx.ReadValue<Vector2>().y);
        };
        //射击状态
        Controls.Keyboard.Aim.started += ctx =>
        {
            int index = weapon.activeWeaponIndex;
            GunController weaponUsing = weapon.GetWeapon(index);
            if (weaponUsing == null) { return; }
            aiming = true;
        };
        Controls.Keyboard.Aim.canceled += ctx =>
        {
            int index = weapon.activeWeaponIndex;
            GunController weaponUsing = weapon.GetWeapon(index);
            if (weaponUsing == null) { return; }
            aiming = false;
        };
        //冲刺状态
        Controls.Keyboard.Sprint.performed += ctx =>
        {
            if (aiming) { running = false; return; }
            running = true;
        };
        Controls.Keyboard.Sprint.canceled += ctx =>
        {
            if (aiming) { running = false; return; }
            running = false;
        };
        //潜行
        Controls.Keyboard.Crouch.performed += ctx =>
        {
            crouching = !crouching;
        };
        //跳跃
        Controls.Keyboard.Jump.performed += ctx =>
        {
            if(Controller.isGrounded||IsGrounded(0.2f)){
                // MoveVec = transform.TransformDirection(MoveVec);
                jumping = true;
            }
        };
        Controller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        Crouched = crouching;
        HandleAnimations();
        HandleGravity();
        HandleMovement();
        HandleCharacterRotation();
    }
    //Move Character On Input
    private void HandleMovement()
    {
        //Establish Direction 建立方向
        MoveVec = transform.TransformDirection(PlayerMoveInput).normalized;
        //Establish Inputs,判断哪个键被按下。
        bool forwardPressed = PlayerMoveInput.z > 0.5;
        bool backwardPressed = PlayerMoveInput.z < -0.5;
        bool leftPressed = PlayerMoveInput.x < -0.5;
        bool rightPressed = PlayerMoveInput.x > 0.5;

        if (controller.CancelAllMovement == true) return;
        if (weapon.CancelAllMovement == true) { return; }
        if (crouching)
        {
            Speed = 0.75f;
            Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            if (forwardPressed || backwardPressed || leftPressed || rightPressed)
            {
                IsCrouching = true;
                IsRunning = false;
                IsWalking = false;
            }
        }
        else
        { 
            if(jumping && !aiming && !IsCrouching)
            {
                if(MoveVec.x!=0||MoveVec.z!=0)
                    MoveVec.y = jumpSpeed;
                else MoveVec.y = jumpSpeed + 10f;
                // 角色在世界中自动下降，模拟重力
                // MoveVec.y -= ConstGravity * Time.deltaTime;
                Controller.Move(MoveVec * Time.deltaTime);
                // Debug.Log(MoveVec);
                IsCrouching = false;
            }
            //向前走
            if (forwardPressed && !running && !backwardPressed)
            {
                IsCrouching = false;
                IsWalking = true;
                IsRunning = false;
                Speed = 1.2f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            //向左、右、后走
            if (!forwardPressed && !running && (leftPressed || rightPressed || backwardPressed))
            {
                IsCrouching = false;
                IsWalking = true;
                IsRunning = false;
                Speed = 1f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            //向前跑
            if (forwardPressed && running && !backwardPressed && !leftPressed && !rightPressed)
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = true;
                Speed = 2f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            //向后跑
            if (!forwardPressed && running && (backwardPressed))
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = true;
                Speed = 1.4f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            //向左右跑
            if (!forwardPressed && running && (leftPressed || rightPressed) && !backwardPressed)
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = true;
                Speed = 1f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            //斜向跑
            if (forwardPressed && running && !backwardPressed && (leftPressed || rightPressed))
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = true;
                Speed = 1.8f;
                //Debug.Log(MoveVec);
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
        }
    }


// 射线检测，这里使用五个射线
    private bool IsGrounded(float distance)
    {
        //pointOffset： 点的偏移位置， distance：检测物体与地面的距离
        float pointOffset= 0.12f;
        bool b = Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z), -Vector3.up, distance);
        bool b1 = Physics.Raycast(new Vector3(transform.position.x - pointOffset, transform.position.y, transform.position.z), -Vector3.up, distance);
        bool b2 = Physics.Raycast(new Vector3(transform.position.x + pointOffset, transform.position.y, transform.position.z), -Vector3.up, distance);
        bool b3 = Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z + pointOffset), -Vector3.up, distance);
        bool b4 = Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z - pointOffset), -Vector3.up, distance);
        return b || b1 || b2 || b3 || b4;
        // return Controller.isGrounded;
    }
    private void HandleGravity()
    {
        //在地面，垂直方向的速度小于0???我并不理解这个重力的处理方式
        if (IsGrounded(0.2f) && velocity.y < 0)
        {
            velocity.y = -2f;
           // Debug.Log("velocity"+velocity);
        }
        velocity.y += ConstGravity * Time.deltaTime; //垂直方向的速度为，v=gt
        Controller.Move(velocity * Time.deltaTime);
    }
    private void HandleAnimations()
    {
        if (jumping)
        {
            animator.SetTrigger("Jumping");
            StartCoroutine(JumpAnimate());
            return;
        }
        if (weapon.CancelAllMovement == true) { return; }
        if (controller.CancelAllMovement == true) return;
        bool forwardPressed = PlayerMoveInput.z > 0.5;
        bool backwardPressed = PlayerMoveInput.z < -0.5;
        bool leftPressed = PlayerMoveInput.x < -0.5;
        bool rightPressed = PlayerMoveInput.x > 0.5;

        if (crouching)
        {
            animator.SetBool("Crouching", true);
            if (forwardPressed && VelocityZ < 0.5f)
            {
                VelocityZ += Time.deltaTime * accleration;
            }
            if (leftPressed && VelocityX > -0.5f)
            {
                VelocityX -= Time.deltaTime * accleration;
            }
            if (rightPressed && VelocityX < 0.5f)
            {
                VelocityX += Time.deltaTime * accleration;
            }
            if (backwardPressed && VelocityZ > -0.5f)
            {
                VelocityZ -= Time.deltaTime * accleration;
            }
            //decrease velocity on axis
            if (!forwardPressed && VelocityZ > 0.0f)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (!backwardPressed && VelocityZ < 0.0f)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            if (!leftPressed && VelocityX < 0.0f)
            {
                VelocityX += Time.deltaTime * deccleration;
            }
            if (!rightPressed && VelocityX > 0.0f)
            {
                VelocityX -= Time.deltaTime * deccleration;
            }
            // reset VelocityZ
            if (!forwardPressed && !backwardPressed && VelocityZ != 0.0f && (VelocityZ > -0.05f && VelocityZ < 0.05f))
            {
                IsCrouching = false;
                VelocityZ = 0.0f;
            }
            // reset VelocityX
            if (!leftPressed && !rightPressed && VelocityX != 0.0f && (VelocityX > -0.05f && VelocityX < 0.05f))
            {
                IsCrouching = false;
                VelocityX = 0.0f;
            }
            // set the parameters to our local variable values
            animator.SetFloat("CrouchingVelocityZ", VelocityZ);
            animator.SetFloat("VelocityX", VelocityX);
        }
        else
        {
            animator.SetBool("Crouching", false);
            //increase velocity on axis
            if (forwardPressed && VelocityZ < 0.5f && !running)
            {
                VelocityZ += Time.deltaTime * accleration;
            }
            if (leftPressed && VelocityX > -0.5f)
            {
                VelocityX -= Time.deltaTime * accleration;
            }
            if (rightPressed && VelocityX < 0.5f)
            {
                VelocityX += Time.deltaTime * accleration;
            }
            if (backwardPressed && VelocityZ > -0.5f && !running)
            {
                VelocityZ -= Time.deltaTime * accleration;
            }
            if (forwardPressed && VelocityZ < 1f && running)
            {
                VelocityZ += Time.deltaTime * accleration;
            }
            if (backwardPressed && VelocityZ > -1f && running)
            {
                VelocityZ -= Time.deltaTime * accleration;
            }
            //Make sure velocity isn't increasing/decreasing
            if (forwardPressed && leftPressed && !running && VelocityZ > 0.5f)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (forwardPressed && rightPressed && !running && VelocityZ > 0.5f)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (backwardPressed && leftPressed && !running && VelocityZ < -0.5f)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            if (backwardPressed && rightPressed && !running && VelocityZ < -0.5f)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            if (forwardPressed && !running && VelocityZ > 0.5f)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (backwardPressed && !running && VelocityZ < -0.5f)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            //decrease velocity on axis
            if (!forwardPressed && VelocityZ > 0.0f)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (!backwardPressed && VelocityZ < 0.0f)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            if (!leftPressed && VelocityX < 0.0f)
            {
                VelocityX += Time.deltaTime * deccleration;
            }
            if (!rightPressed && VelocityX > 0.0f)
            {
                VelocityX -= Time.deltaTime * deccleration;
            }
            // reset VelocityZ
            if (!forwardPressed && !backwardPressed && VelocityZ != 0.0f && (VelocityZ > -0.05f && VelocityZ < 0.05f))
            {
                IsCrouching = false;
                IsRunning = false;
                IsWalking = false;
                VelocityZ = 0.0f;
            }
            // reset VelocityX
            if (!leftPressed && !rightPressed && VelocityX != 0.0f && (VelocityX > -0.05f && VelocityX < 0.05f))
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = false;
                VelocityX = 0.0f;
            }
            // set the parameters to our local variable values
            animator.SetFloat("StandingVelocityZ", VelocityZ);
            animator.SetFloat("VelocityX", VelocityX);
        }
    }
    IEnumerator JumpAnimate()
    {
        yield return new WaitForSeconds(12*Time.deltaTime);
        jumping = false;
        animator.ResetTrigger("Jumping");
    }

    //处理角色转向，此处引起的bug是由于武器目标约束的存在，并且没有限制武器何时不再跟随目标，导致双手穿过身体
    private void HandleCharacterRotation()
    {
        if (controller.CancelAllMovement == true) return;
        //  if(IsWalking||IsRunning){
            float yawCamera = cam.transform.rotation.eulerAngles.y;
            //turnSmoothVelocity 当前速度，此值由函数在每次调用时进行修改。
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, yawCamera, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation=Quaternion.Euler(0f,angle,0f);
        
    }

    public void Death()
    {
        controller.CancelAllMovement = true;
        Controls.Disable();
        animator.SetTrigger("Died");
        Debug.Log("You are Died!");
    }

}
