using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] private bool LockCursor;
    private void Start()
    {
        //锁定/隐藏光标
        Cursor.visible = false;
        //将光标锁定在该游戏窗口的中心。
        Cursor.lockState = CursorLockMode.Locked;
    }
    void LateUpdate()
    {
        //CancelAllMovement 取消移动标志，该变量为true时，角色不移动，光标显示
        if (FindObjectOfType<UIController>().CancelAllMovement == true)
        {
           Cursor.visible = true;
           Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
