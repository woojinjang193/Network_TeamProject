using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireModule
{
    private AIController _controller;
    private float nextFireTime = 0f;

    public FireModule(AIController controller)
    {
        _controller = controller;
    }
    public void FireAt(Transform target)
    {
        if(Time.time >= nextFireTime)
        {
            Vector3 dir = (target.position - _controller.transform.position).normalized;
            Debug.DrawRay(_controller.transform.position, dir * 5f, Color.red, 0.2f); //TODO 플레이어 기능의 물감발사를 가져오자 + 발사간격
            //Debug.Log($"발사 방향: + {dir}, 타겟 위치: + { target.position}");
            nextFireTime = Time.time + _controller.fireInterval;
        }
    }
}
