using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//射击检测
// 为了利用射线实现子弹击中目标的检测，世界需要用透明的墙包裹。
public class CrossHairTarget : MonoBehaviour
{
    Camera camera;
    // Ray:在程序中可以理解为射线，就是以某个位置（origin)朝某个方向(direction)的一条射线；
    Ray ray;
    // RaycastHit，它是用于存储射线碰撞到的第一个物体的信息,所以需要提前创建这个对象，用于信息的碰撞信息的存储；
    //     collider与射线发生碰撞的碰撞器
    // distance 从射线起点到射线与碰撞器的交点的距离
    // normal 射线射入平面的法向量
    // point 射线与碰撞器交点的坐标（Vector3对象）
    RaycastHit hitinfo;
    
    LayerMask layerMask;
    void Start()
    {
        layerMask = LayerMask.GetMask("FieldSphere");
        camera = GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // 射线起点
        ray.origin = camera.transform.position;
        // 射线方向
        ray.direction = camera.transform.forward;
        if (Physics.Raycast(ray, out hitinfo,1000f,~layerMask))
        {
            transform.position = hitinfo.point;
        }
        else
        {
            transform.position = ray.origin + ray.direction * 1000.0f;
        }
    }
}
